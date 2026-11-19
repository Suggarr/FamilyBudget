using System.ComponentModel.DataAnnotations;
using FamilyBudget.Core.Models;
using FamilyBudget.Core.Models.Auth;

namespace FamilyBudget.Application.Dtos.Auth
{
    public sealed class RegisterLocalRequest
    {
        [Required]
        [StringLength(User.MAX_NAME_LENGTH)]
        public string DisplayName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(LocalCredential.MaxEmailLength)]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;

        [Required]
        [Compare(nameof(Password))]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
