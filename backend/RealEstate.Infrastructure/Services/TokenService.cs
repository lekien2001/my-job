using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using RealEstate.Core.Entities;
using RealEstate.Core.Interfaces;

namespace RealEstate.Infrastructure.Services
{
    public class TokenService : ITokenService
    {
        private readonly string _secretKey;
        private readonly string _issuer;
        private readonly string _audience;
        private readonly int _expiryInMinutes;

        public TokenService(IConfiguration configuration)
        {
            var jwtSettings = configuration.GetSection("JwtSettings");
            _secretKey = jwtSettings["Secret"] 
                ?? throw new InvalidOperationException("JWT Secret key is not configured.");
            _issuer = jwtSettings["Issuer"] ?? "RealEstateApi";
            _audience = jwtSettings["Audience"] ?? "RealEstateClient";
            _expiryInMinutes = int.TryParse(jwtSettings["ExpiryInMinutes"], out var expiry) ? expiry : 1440;
        }

        public string GenerateJwtToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_secretKey);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.FullName),
                    new Claim(ClaimTypes.Role, user.Role),
                    new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
                    new Claim(ClaimTypes.MobilePhone, user.PhoneNumber ?? string.Empty)
                }),
                Expires = DateTime.UtcNow.AddMinutes(_expiryInMinutes),
                Issuer = _issuer,
                Audience = _audience,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
