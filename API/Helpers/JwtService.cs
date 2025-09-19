using Domain.Entities;
using Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace API.Helpers
{
    public class JwtService
    {
        private readonly string _secret;
        private readonly string _issuer;
        private readonly string _audience;
        private readonly int _expireMinutes;
        private readonly IUnitOfWork _unitOfWork;

        public JwtService(IConfiguration config,IUnitOfWork unitOfWork)
        {
            _secret = config["Jwt:Key"]!;
            _issuer = config["Jwt:Issuer"]!;
            _audience = config["Jwt:Audience"]!;
            _expireMinutes = int.Parse(config["Jwt:ExpireMinutes"]!);
            _unitOfWork = unitOfWork;
        }

        public string GenerateToken(User user)
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim("username", user.Username)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _issuer,
                audience: _audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_expireMinutes),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        // Generate Refresh Token
        public (string Token, DateTime Expires) GenerateRefreshToken()
        {
            var randomBytes = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);

            return (
                Token: Convert.ToBase64String(randomBytes),
                Expires: DateTime.UtcNow.AddDays(7)
            );
        }

    

        public bool ValidateRefreshToken(User user, string refreshToken)
        {
            return user.RefreshToken == refreshToken &&
                   user.RefreshTokenExpiryTime > DateTime.UtcNow;
        }
    }
}
