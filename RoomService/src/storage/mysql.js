import * as Y from 'yjs'
import mysql from 'mysql2/promise'
import * as env from 'lib0/environment.js'

/**
 * @typedef {import('../storage.js').AbstractStorage} AbstractStorage
 */

/**
 * @param {Object} [conf]
 * @param {string} [conf.url] mysql://user:password@host:port/database (defaults to the MYSQL env var)
 */
export const createMySqlStorage = async ({ url = env.ensureConf('mysql') } = {}) => {
  const pool = mysql.createPool({ uri: url, connectionLimit: 10 })
  await pool.query(`
    CREATE TABLE IF NOT EXISTS yjs_docs (
      r       BIGINT NOT NULL AUTO_INCREMENT PRIMARY KEY,
      room    VARCHAR(255) NOT NULL,
      doc     VARCHAR(255) NOT NULL,
      branch  VARCHAR(255) NOT NULL DEFAULT 'main',
      gc      BOOLEAN NOT NULL DEFAULT TRUE,
      \`update\` LONGBLOB NOT NULL,
      sv      BLOB NOT NULL,
      INDEX idx_yjs_docs_doc (room, doc, branch, gc, r)
    )
  `)
  return new MySqlStorage(pool)
}

/**
 * Persists documents in MySQL, so the room documents live in the same database as the rest of
 * TeamSketch. Each persisted state is one row; the worker merges rows and deletes the old ones.
 *
 * @implements AbstractStorage
 */
class MySqlStorage {
  /**
   * @param {mysql.Pool} pool
   */
  constructor (pool) {
    this.pool = pool
  }

  /**
   * @param {string} room
   * @param {string} docname
   * @param {Y.Doc} ydoc
   * @param {Object} opts
   * @param {boolean} [opts.gc]
   * @param {string} [opts.branch]
   * @returns {Promise<void>}
   */
  async persistDoc (room, docname, ydoc, { gc = true, branch = 'main' } = {}) {
    await this.pool.execute(
      'INSERT INTO yjs_docs (room, doc, branch, gc, `update`, sv) VALUES (?, ?, ?, ?, ?, ?)',
      [room, docname, branch, gc, Buffer.from(Y.encodeStateAsUpdateV2(ydoc)), Buffer.from(Y.encodeStateVector(ydoc))]
    )
  }

  /**
   * @param {string} room
   * @param {string} docname
   * @param {Object} opts
   * @param {boolean} [opts.gc]
   * @param {string} [opts.branch]
   * @return {Promise<{ doc: Uint8Array, references: Array<number> } | null>}
   */
  async retrieveDoc (room, docname, { gc = true, branch = 'main' } = {}) {
    const [rows] = /** @type {[Array<{ r: number, update: Buffer }>, any]} */ (await this.pool.execute(
      'SELECT r, `update` FROM yjs_docs WHERE room = ? AND doc = ? AND branch = ? AND gc = ? ORDER BY r',
      [room, docname, branch, gc]
    ))
    if (rows.length === 0) {
      return null
    }
    const doc = Y.mergeUpdatesV2(rows.map(row => new Uint8Array(row.update)))
    const references = rows.map(row => row.r)
    return { doc, references }
  }

  /**
   * @param {string} room
   * @param {string} docname
   * @param {Object} opts
   * @param {boolean} [opts.gc]
   * @param {string} [opts.branch]
   * @return {Promise<Uint8Array|null>}
   */
  async retrieveStateVector (room, docname, { gc = true, branch = 'main' } = {}) {
    const [rows] = /** @type {[Array<{ sv: Buffer }>, any]} */ (await this.pool.execute(
      'SELECT sv FROM yjs_docs WHERE room = ? AND doc = ? AND branch = ? AND gc = ? ORDER BY r DESC LIMIT 1',
      [room, docname, branch, gc]
    ))
    return rows.length === 0 ? null : new Uint8Array(rows[0].sv)
  }

  /**
   * @param {string} room
   * @param {string} docname
   * @param {Array<any>} storeReferences
   * @param {Object} opts
   * @param {boolean} [opts.gc]
   * @param {string} [opts.branch]
   * @return {Promise<void>}
   */
  async deleteReferences (room, docname, storeReferences, { gc = true, branch = 'main' } = {}) {
    if (storeReferences.length === 0) return
    await this.pool.query(
      'DELETE FROM yjs_docs WHERE room = ? AND doc = ? AND branch = ? AND gc = ? AND r IN (?)',
      [room, docname, branch, gc, storeReferences]
    )
  }

  async destroy () {
    await this.pool.end()
  }
}

export const Storage = MySqlStorage
