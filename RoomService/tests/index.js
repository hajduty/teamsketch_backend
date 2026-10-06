/* eslint-env node */

import * as api from './api.tests.js'
import * as storage from './storage.tests.js'
import { runTests } from 'lib0/testing.js'

runTests({
  storage,
  api
}).then(success => {
  process.exit(success ? 0 : 1)
})
