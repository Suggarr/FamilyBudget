using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyBudget.Infrastructure.Authentication
{
    public class JwtOptions
    {
        [Required]
        public string Issuer { get; set; } = null!;

        [Required]
        public string Audience { get; set; } = null!;

        [Required]
        public string SecretKey { get; set; } = null!;

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "AccessTokenLifetimeMinutes")]
        public int AccessTokenLifetimeMinutes { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "RefreshTokenLifetimeDays")]
        public int RefreshTokenLifetimeDays { get; set; }
    }
}
