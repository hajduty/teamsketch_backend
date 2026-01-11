import * as uws from 'uws'
import * as logging from 'lib0/logging'
import * as error from 'lib0/error.js'
import { registerYWebsocketServer } from './ws.js'
import * as promise from 'lib0/promise.js'
// @ts-ignore
import { checkPermissionFromUrl } from './grpcClient.js'
import './redisSubscription.js'

//const wsServerPublicKey = await ecdsa.importKeyJwk(json.parse(env.ensureConf('auth-public-key')))
// const wsServerPrivateKey = await ecdsa.importKeyJwk(json.parse(env.ensureConf('auth-private-key')))

class YWebsocketServer {
  /**
   * @param {uws.TemplatedApp} app
   */
  constructor (app) {
    this.app = app
  }

  async destroy () {
    this.app.close()
  }
}

/**
 * @param {Object} opts
 * @param {number} opts.port
 * @param {import('./storage.js').AbstractStorage} opts.store
 * @param {string} [opts.redisPrefix]
 * @param {(room:string,docname:string,client:import('./api.js').Api)=>void} [opts.initDocCallback] -
// this is called when a doc is accessed, but it doesn't exist. You could populate the doc here.
// However, this function could be called several times, until some content exists. So you need to
// handle concurrent calls.
 */
export const createYWebsocketServer = async ({
  redisPrefix = 'y',
  port,
  store,
  initDocCallback = () => {}
}) => {
  const app = uws.App({})
  await registerYWebsocketServer(app, '/:room/:token', store, async (req) => {
    const room = /** @type {string} */ (req.getParameter(0))
    const token = /** @type {string} */ (req.getParameter(1))
    // Parse gc and branch query parameters BEFORE any await
    const gc = req.getQuery('gc') !== 'false' // default to true unless explicitly set to 'false'
    const branch = req.getQuery('branch') || 'main'
    if (token == null) {
      throw new Error('Missing Token')
    }
    // verify that the user has a valid token
/*     const { payload: userToken } = await jwt.verifyJwt(wsServerPublicKey, token)
    if (userToken.yuserid == null) {
      throw new Error('Missing userid in user token!')
    } */
    try {
      const perm = await checkPermissionFromUrl(room, token)
      if (!perm) {
        throw new Error('Permission denied')
      }
      return { hasWriteAccess: perm.role === 'Owner' || perm.role === 'Editor', room, userid: perm.userId || '', gc, branch }
    } catch (e) {
      console.error('Failed to check permissions via gRPC', e)
      throw e
    }
  }, { redisPrefix, initDocCallback })

  await promise.create((resolve, reject) => {
    app.listen(port, (token) => {
      if (token) {
        logging.print(logging.GREEN, '[y-redis] Listening to port ', port)
        resolve()
      } else {
        const err = error.create('[y-redis] Failed to lisen to port ' + port)
        reject(err)
        throw err
      }
    })
  })
  return new YWebsocketServer(app)
}
