using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace LearningReport.Models
{
    [Index(nameof(Email), IsUnique = true)]
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(100), EmailAddress]
        public required string Email { get; set; }

        [Required, MinLength(5), MaxLength(100)]
        public required string Password { get; set; }

        [RegularExpression("Admin|User"), MaxLength(100)]
        public string Role { get; set; } = "User";
    }
}
