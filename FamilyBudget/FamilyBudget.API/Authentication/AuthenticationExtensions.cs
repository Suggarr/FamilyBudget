using FamilyBudget.Infrastructure.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace FamilyBudget.API.Authentication
{
    public static class AuthenticationExtensions
    {
        public static IServiceCollection AddJwtAuthentication(
            this IServiceCollection services, 
            IConfiguration configuration)
        {
            services.AddOptions<JwtOptions>()
                .Bind(configuration.GetSection("JwtSettings"))
                .ValidateDataAnnotations() // проверки на основе атрибутов в JwtOptions
                .Validate(
                    options => !string.IsNullOrWhiteSpace(options.SecretKey)
                        && Encoding.UTF8.GetByteCount(options.SecretKey) >= 32,
                    "JWT secret key must contain at least 32 bytes.")
                .ValidateOnStart();

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer();

            services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
                .Configure<IOptions<JwtOptions>>((bearer, jwtOptions) =>
                {
                    var jwt = jwtOptions.Value;
                    bearer.MapInboundClaims = false;

                    bearer.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = jwt.Issuer,

                        ValidateAudience = true,
                        ValidAudience = jwt.Audience,

                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.Zero,

                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SecretKey)),

                        ValidAlgorithms = new[] { SecurityAlgorithms.HmacSha256 }
                    };
                });

            return services;
        }
    }
}
