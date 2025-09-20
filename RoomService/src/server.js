#!/usr/bin/env node

import http from 'http'
import WebSocket from 'ws'
import { LeveldbPersistence } from 'y-leveldb'
import { checkPermissionFromUrl } from './grpcClient.js'
import * as number from 'lib0/number'
import * as Y from 'yjs'
import { setPersistence, setupWSConnection } from './utils.js'

const host = process.env.HOST || 'localhost'
const port = number.parseInt(process.env.PORT || '1234')

const ldb = new LeveldbPersistence('./yjs-database')

setPersistence({
  provider: ldb,

  bindState: async (docName, ydoc) => {
    const persistedDoc = await ldb.getYDoc(docName)
    const state = Y.encodeStateAsUpdate(persistedDoc)
    Y.applyUpdate(ydoc, state)

    ydoc.on('update', update => {
      ldb.storeUpdate(docName, update)
    })
  },

  writeState: async (_docName, _ydoc) => Promise.resolve()
})

const server = http.createServer((_req, res) => {
  res.writeHead(200, { 'Content-Type': 'text/plain' })
  res.end('okay')
})

const wss = new WebSocket.Server({ noServer: true })

wss.on('connection', async (ws, req) => {
  await setupWSConnection(ws, req)
  console.log(`New connection to room "${req.url?.slice(1) || 'default-room'}"`)
})

server.on('upgrade', async (request, socket, head) => {
  const userInfo = await checkPermissionFromUrl(request.url)

  if (!userInfo) {
    socket.write("HTTP/1.1 403 Forbidden\r\n\r\n")
    socket.destroy()
    return
  }

  wss.handleUpgrade(request, socket, head, ws => {
    wss.emit('connection', ws, request)
  })
})

server.listen(port, host, () => {
  console.log(`running at '${host}' on port ${port}`)
})
