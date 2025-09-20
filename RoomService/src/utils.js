import * as Y from 'yjs'
import * as syncProtocol from '@y/protocols/sync'
import * as awarenessProtocol from '@y/protocols/awareness'

import * as encoding from 'lib0/encoding'
import * as decoding from 'lib0/decoding'
// @ts-ignore
import * as map from 'lib0/map'

import * as eventloop from 'lib0/eventloop'

import { callbackHandler, isCallbackSet } from './callback.js'

const CALLBACK_DEBOUNCE_WAIT = parseInt(process.env.CALLBACK_DEBOUNCE_WAIT || '2000')
const CALLBACK_DEBOUNCE_MAXWAIT = parseInt(process.env.CALLBACK_DEBOUNCE_MAXWAIT || '10000')
const debouncer = eventloop.createDebouncer(CALLBACK_DEBOUNCE_WAIT, CALLBACK_DEBOUNCE_MAXWAIT)

const wsReadyStateConnecting = 0
const wsReadyStateOpen = 1
// @ts-ignore
const wsReadyStateClosing = 2
// @ts-ignore
const wsReadyStateClosed = 3

const gcEnabled = process.env.GC !== 'false' && process.env.GC !== '0'

/** @type {{bindState: function(string,WSSharedDoc):void, writeState:function(string,WSSharedDoc):Promise<any>, provider: any}|null} */
let persistence = null
export const setPersistence = persistence_ => { persistence = persistence_ }
export const getPersistence = () => persistence

/** @type {Map<string,WSSharedDoc>} */
export const docs = new Map()

const messageSync = 0
const messageAwareness = 1

const updateHandler = (update, _origin, doc, _tr) => {
  const encoder = encoding.createEncoder()
  encoding.writeVarUint(encoder, messageSync)
  syncProtocol.writeUpdate(encoder, update)
  const message = encoding.toUint8Array(encoder)
  doc.conns.forEach((_, conn) => send(doc, conn, message))
}

let contentInitializor = _ydoc => Promise.resolve()
export const setContentInitializor = (f) => { contentInitializor = f }

export class WSSharedDoc extends Y.Doc {
  constructor (name) {
    super({ gc: gcEnabled })
    this.name = name
    this.conns = new Map()
    this.awareness = new awarenessProtocol.Awareness(this)
    this.awareness.setLocalState(null)

    const awarenessChangeHandler = ({ added, updated, removed }, conn) => {
      const changedClients = added.concat(updated, removed)
      if (conn !== null) {
        const connControlledIDs = this.conns.get(conn)
        if (connControlledIDs !== undefined) {
          added.forEach(clientID => { connControlledIDs.add(clientID) })
          removed.forEach(clientID => { connControlledIDs.delete(clientID) })
        }
      }

      const encoder = encoding.createEncoder()
      encoding.writeVarUint(encoder, messageAwareness)
      encoding.writeVarUint8Array(encoder, awarenessProtocol.encodeAwarenessUpdate(this.awareness, changedClients))
      const buff = encoding.toUint8Array(encoder)
      this.conns.forEach((_, c) => send(this, c, buff))
    }

    this.awareness.on('update', awarenessChangeHandler)
    this.on('update', updateHandler)

    if (isCallbackSet) {
      this.on('update', (_update, _origin, doc) => {
        // @ts-ignore
        debouncer(() => callbackHandler(doc))
      })
    }

    this.whenInitialized = contentInitializor(this)
  }
}

export const getYDoc = async (docname, gc = true) => {
  if (docs.has(docname)) return docs.get(docname)

  const doc = new WSSharedDoc(docname)
  doc.gc = gc
  docs.set(docname, doc)

  if (persistence !== null) {
    try {
      await persistence.bindState(docname, doc)
    } catch (err) {
      console.error('Error in persistence.bindState for', docname, err)
    }
  }

  return doc
}

const messageListener = (conn, doc, message) => {
  try {
    const encoder = encoding.createEncoder()
    const decoder = decoding.createDecoder(message)
    const messageType = decoding.readVarUint(decoder)

    switch (messageType) {
      case messageSync:
        encoding.writeVarUint(encoder, messageSync)
        syncProtocol.readSyncMessage(decoder, encoder, doc, conn)
        if (encoding.length(encoder) > 1) send(doc, conn, encoding.toUint8Array(encoder))
        break
      case messageAwareness:
        awarenessProtocol.applyAwarenessUpdate(doc.awareness, decoding.readVarUint8Array(decoder), conn)
        break
    }
  } catch (err) {
    console.error(err)
    doc.emit('error', [err])
  }
}

const closeConn = (doc, conn) => {
  if (!doc.conns.has(conn)) return
  const controlledIds = doc.conns.get(conn)
  doc.conns.delete(conn)
  awarenessProtocol.removeAwarenessStates(doc.awareness, Array.from(controlledIds), null)
  if (doc.conns.size === 0 && persistence !== null) {
    persistence.writeState(doc.name, doc).then(() => {
      doc.destroy()
    })
    docs.delete(doc.name)
  }
  conn.close()
}

const send = (doc, conn, m) => {
  if (![wsReadyStateConnecting, wsReadyStateOpen].includes(conn.readyState)) {
    closeConn(doc, conn)
    return
  }
  try { conn.send(m, {}, err => { if (err) closeConn(doc, conn) }) }
  catch { closeConn(doc, conn) }
}

const pingTimeout = 30000

export const setupWSConnection = async (conn,req,{ gc = true } = {}) => {
  conn.binaryType = 'arraybuffer'

  // Split URL path: /roomId/JWT_TOKEN
  const urlParts = (req.url || '').slice(1).split('/')
  const roomName = urlParts[0] || 'default-room'

  const doc = await getYDoc(roomName, gc)
  // @ts-ignore
  doc.conns.set(conn, new Set())

  conn.on('message', message => messageListener(conn, doc, new Uint8Array(message)))

  let pongReceived = true
  const pingInterval = setInterval(() => {
    // @ts-ignore
    if (!pongReceived) { if (doc.conns.has(conn)) closeConn(doc, conn); clearInterval(pingInterval) }
    // @ts-ignore
    else if (doc.conns.has(conn)) {
      pongReceived = false
      try { conn.ping() } catch { closeConn(doc, conn); clearInterval(pingInterval) }
    }
  }, pingTimeout)

  conn.on('close', () => { closeConn(doc, conn); clearInterval(pingInterval) })
  conn.on('pong', () => { pongReceived = true })

  // send sync step 1
  const encoder = encoding.createEncoder()
  encoding.writeVarUint(encoder, messageSync)
  // @ts-ignore
  syncProtocol.writeSyncStep1(encoder, doc)
  send(doc, conn, encoding.toUint8Array(encoder))

  // @ts-ignore
  const awarenessStates = doc.awareness.getStates()
  if (awarenessStates.size > 0) {
    const encoder2 = encoding.createEncoder()
    encoding.writeVarUint(encoder2, messageAwareness)
    // @ts-ignore
    encoding.writeVarUint8Array(encoder2, awarenessProtocol.encodeAwarenessUpdate(doc.awareness, Array.from(awarenessStates.keys())))
    send(doc, conn, encoding.toUint8Array(encoder2))
  }
}
