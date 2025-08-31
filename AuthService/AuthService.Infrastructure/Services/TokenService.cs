using AuthService.Core.Interfaces;
using AuthService.Infrastructure.Helpers;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace AuthService.Infrastructure.Services;

public class TokenService : ITokenService
{
    private readonly RSA _privateRsa;
    private readonly RsaSecurityKey _publicKey;
    private readonly IConfiguration _configuration;

    public TokenService(IConfiguration configuration)
    {
        _privateRsa = RsaKeyHelper.LoadOrCreate(configuration);
        _publicKey = new RsaSecurityKey(_privateRsa.ExportParameters(false));
        _configuration = configuration;
    }

    public string GenerateRefreshToken(int userId, string email)
    {
        throw new NotImplementedException();
    }

    public string GenerateToken(int userId, string email)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, email)
        };

        var creds = new SigningCredentials(new RsaSecurityKey(_privateRsa) { KeyId = "auth-key-1"}, SecurityAlgorithms.RsaSha256);

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddDays(7),
            signingCredentials: creds
            );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public bool IsTokenValid(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();

        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidIssuer = _configuration["Jwt:Issuer"],
            ValidAudience = _configuration["Jwt:Audience"],
            IssuerSigningKey = _publicKey
        };
        
        var principal = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);
        
        return validatedToken is JwtSecurityToken jwt && jwt.Header.Alg.Equals(SecurityAlgorithms.RsaSha256, StringComparison.InvariantCultureIgnoreCase);
    }
}