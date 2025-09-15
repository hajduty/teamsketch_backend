using AuthService.Infrastructure.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;

namespace AuthService.API.Controllers
{
    [ApiController]
    [Route(".well-known")]
    public class JwksController(IConfiguration config) : ControllerBase
    {

        [HttpGet("jwks.json")]
        public IActionResult GetJwks()
        {
            var rsa = RsaKeyHelper.GetRsa(config);
            var key = new RsaSecurityKey(rsa)
            {
                KeyId = "auth-key-1"
            };

            var jwk = new JsonWebKey
            {
                Kty = "RSA",
                Kid = key.KeyId,
                Use = "sig",
                Alg = SecurityAlgorithms.RsaSha256,
                N = Base64UrlEncoder.Encode(rsa.ExportParameters(false).Modulus),
                E = Base64UrlEncoder.Encode(rsa.ExportParameters(false).Exponent)
            };

            return Ok(new { keys = new[] { jwk } });
        }
    }
}
