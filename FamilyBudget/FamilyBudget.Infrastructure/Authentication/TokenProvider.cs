using FamilyBudget.Core.Interfaces;
using FamilyBudget.Core.Models.Auth;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace FamilyBudget.Infrastructure.Authentication
{
    public class TokenProvider : ITokenProvider
    {
        private readonly JwtOptions _options;

        public TokenProvider(IOptions<JwtOptions> options)
        {
            _options = options.Value;
        }

        public TokenResult GenerateAccessToken(Guid accountId, Guid sessionId)
        {
            if (accountId == Guid.Empty)
            {
                throw new ArgumentException("AccountId cannot be empty.", nameof(accountId));
            }

            if (sessionId == Guid.Empty)
            {
                throw new ArgumentException("SessionId cannot be empty.", nameof(sessionId));
            }

            var utcNow = DateTimeOffset.UtcNow;
            var expiresAtUtc = utcNow.AddMinutes(_options.AccessTokenLifetimeMinutes);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, accountId.ToString()),
                new Claim("sid", sessionId.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SecretKey));

            var descriptor = new SecurityTokenDescriptor
            {
                Issuer = _options.Issuer,
                Audience = _options.Audience,
                Subject = new ClaimsIdentity(claims),
                IssuedAt = utcNow.UtcDateTime,
                NotBefore = utcNow.UtcDateTime,
                Expires = expiresAtUtc.UtcDateTime,
                SigningCredentials = new SigningCredentials(
                    key,
                    SecurityAlgorithms.HmacSha256)
            };

            var token = new JsonWebTokenHandler().CreateToken(descriptor);

            return new TokenResult(token, expiresAtUtc);
        }
    }
}
