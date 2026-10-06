#!/usr/bin/env node

import * as env from 'lib0/environment.js'
import { createWorker } from '../src/api.js'
import { createStoreFromEnv } from '../src/store.js'

const redisPrefix = env.getConf('redis-prefix') || 'y'

const store = await createStoreFromEnv()

await createWorker(store, redisPrefix, {})
