import * as env from 'lib0/environment.js'

/**
 * Picks the persistent storage from the environment. The first configured backend wins:
 * S3_ENDPOINT -> S3, MYSQL -> MySQL, POSTGRES -> Postgres, otherwise in-memory (single instance only).
 *
 * @return {Promise<import('./storage.js').AbstractStorage>}
 */
export const createStoreFromEnv = async () => {
  if (env.getConf('s3-endpoint')) {
    console.log('using s3 store')
    const { createS3Storage } = await import('./storage/s3.js')
    const bucketName = env.getConf('s3-bucket') || 'images'
    const store = createS3Storage(bucketName)
    try {
      // make sure the bucket exists
      await store.client.makeBucket(bucketName)
    } catch (e) {}
    return store
  }
  if (env.getConf('mysql')) {
    console.log('using mysql store')
    const { createMySqlStorage } = await import('./storage/mysql.js')
    return createMySqlStorage()
  }
  if (env.getConf('postgres')) {
    console.log('using postgres store')
    const { createPostgresStorage } = await import('./storage/postgres.js')
    return createPostgresStorage()
  }
  console.log('ATTENTION! using in-memory store')
  const { createMemoryStorage } = await import('./storage/memory.js')
  return createMemoryStorage()
}
