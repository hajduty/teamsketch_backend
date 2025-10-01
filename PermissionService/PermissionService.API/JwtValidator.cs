using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace PermissionService.API
{
    public class JwtValidator
    {
        private readonly string _authServiceUrl;
        private readonly HttpClient _httpClient;

        public static HttpClient? TestHttpClient { get; set; }

        public JwtValidator(string authServiceUrl, HttpClient? httpClient = null)
        {
            _authServiceUrl = authServiceUrl ?? throw new ArgumentNullException(nameof(authServiceUrl));

            _httpClient = httpClient ?? TestHttpClient ?? new HttpClient();
        }

        public async Task<JsonWebKeySet> GetJwksAsync()
        {
            var jwksUrl = $"{_authServiceUrl}/.well-known/jwks.json";
            var jwksJson = await _httpClient.GetStringAsync(jwksUrl);
            return new JsonWebKeySet(jwksJson);
        }
    }
}
