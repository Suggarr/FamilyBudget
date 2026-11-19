using FamilyBudget.Core.Models.Auth;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyBudget.Application.Dtos.Auth
{
    public sealed class LoginRequest
    {
        [Required]
        [EmailAddress]
        [StringLength(LocalCredential.MaxEmailLength)]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;

        [Required]
        [StringLength(AuthSession.MaxDeviceIdLength)]
        public string DeviceId { get; set; } = string.Empty;

        [Required]
        [StringLength(AuthSession.MaxDeviceNameLength)]
        public string DeviceName { get; set; } = string.Empty;
    }
}
