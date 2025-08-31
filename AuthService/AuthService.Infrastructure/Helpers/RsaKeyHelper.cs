using Microsoft.Extensions.Configuration;
using System.Security.Cryptography;

namespace AuthService.Infrastructure.Helpers
{
    public static class RsaKeyHelper
    {
        public static RSA LoadOrCreate(IConfiguration config)
        {
            var rsa = RSA.Create(2048);
            var keypath = config["Jwt:KeyPath"];

            if (File.Exists(keypath))
            {
                var keyText = File.ReadAllText(keypath);
                rsa.FromXmlString(keyText);
            }
            else
            {
                var keyText = rsa.ToXmlString(true);
                File.WriteAllText(keypath!, keyText);
            }

            return rsa;
        }
    }
}
