#!/usr/bin/env node

import * as number from 'lib0/number.js'
import * as env from 'lib0/environment.js'
import { createYWebsocketServer } from '../src/server.js'
import { createStoreFromEnv } from '../src/store.js'

const port = number.parseInt(env.getConf('port') || '3002')
const redisPrefix = env.getConf('redis-prefix') || 'y'

const store = await createStoreFromEnv()

await createYWebsocketServer({ port, store, redisPrefix })
