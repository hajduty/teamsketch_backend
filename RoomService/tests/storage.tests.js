import * as t from 'lib0/testing.js'
import * as Y from 'yjs'
import * as env from 'lib0/environment.js'
import { createMemoryStorage } from '../src/storage/memory.js'

/**
 * Runs the same contract against every storage backend that is configured in the environment.
 * Memory always runs; MySQL, Postgres and S3 run when MYSQL, POSTGRES_TESTDB or S3_ENDPOINT are set.
 *
 * @param {t.TestCase} _tc
 */
export const testStorages = async _tc => {
  /**
   * @type {Object<string, import('../src/storage.js').AbstractStorage>}
   */
  const storages = { memory: createMemoryStorage() }
  if (env.getConf('mysql')) {
    const { createMySqlStorage } = await import('../src/storage/mysql.js')
    const mysql = await createMySqlStorage()
    await mysql.pool.query("DELETE FROM yjs_docs WHERE room IN ('room', 'nonexistend')")
    storages.mysql = mysql
  }
  if (env.getConf('postgres-testdb')) {
    const { createPostgresStorage } = await import('../src/storage/postgres.js')
    const postgres = await createPostgresStorage({ database: env.ensureConf('postgres-testdb') })
    await postgres.sql`DELETE from yredis_docs_v2`
    storages.postgres = postgres
  }
  if (env.getConf('s3-endpoint')) {
    const { createS3Storage } = await import('../src/storage/s3.js')
    const s3TestBucketName = 'yredis-tests'
    const s3 = createS3Storage(s3TestBucketName)
    try {
      await s3.client.makeBucket(s3TestBucketName)
    } catch (e) {}
    try {
      const files = await s3.client.listObjectsV2(s3TestBucketName, '', true).toArray()
      await s3.client.removeObjects(s3TestBucketName, files.map(file => file.name))
    } catch (e) {}
    storages.s3 = s3
  }
  for (const storageName in storages) {
    const storage = storages[storageName]
    await t.groupAsync(`storage: ${storageName}`, async () => {
      {
        t.info('persisting docs')
        // index doc for baseline
        const ydoc1 = new Y.Doc()
        ydoc1.getMap().set('a', 1)
        await storage.persistDoc('room', 'index', ydoc1)
        const sv1 = await storage.retrieveStateVector('room', 'index')
        t.assert(sv1)
        t.compare(new Uint8Array(sv1), Y.encodeStateVector(ydoc1), 'state vectors match')
        // second doc with different changes under the same index key
        const ydoc2 = new Y.Doc()
        ydoc2.getMap().set('b', 1)
        await storage.persistDoc('room', 'index', ydoc2)
        // third doc that will be stored under a different key
        const ydoc3 = new Y.Doc()
        ydoc3.getMap().set('a', 2)
        await storage.persistDoc('room', 'doc3', ydoc3)
        const sv2 = await storage.retrieveStateVector('room', 'doc3')
        t.assert(sv2)
        t.compare(new Uint8Array(sv2), Y.encodeStateVector(ydoc3), 'state vectors match')
      }
      {
        t.info('retrieving docs')
        const r1 = await storage.retrieveDoc('room', 'index')
        t.assert(r1)
        t.assert(r1.references.length === 2) // we stored two different versions that should be merged now
        const doc1 = new Y.Doc()
        Y.applyUpdateV2(doc1, r1.doc)
        // should have merged both changes..
        t.assert(doc1.getMap().get('a') === 1 && doc1.getMap().get('b') === 1)
        // retrieve other doc..
        const doc3 = new Y.Doc()
        const r3 = await storage.retrieveDoc('room', 'doc3')
        t.assert(r3)
        t.assert(r3.references.length === 1)
        Y.applyUpdateV2(doc3, r3.doc)
        t.assert(doc3.getMap().get('a') === 2)
        t.info('delete references')
        await storage.deleteReferences('room', 'index', [r1.references[0]])
        const r1v2 = await storage.retrieveDoc('room', 'index')
        t.assert(r1v2 && r1v2.references.length === 1)
        await storage.deleteReferences('room', 'index', [r1.references[1]])
        const r1v3 = await storage.retrieveDoc('room', 'index')
        t.assert(r1v3 == null)
      }
      {
        const sv = await storage.retrieveStateVector('nonexistend', 'nonexistend')
        t.assert(sv === null)
      }
      await storage.destroy()
    })
  }
}
