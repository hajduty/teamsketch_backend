using System.ComponentModel.DataAnnotations;

namespace UserService.Core.Entities;

public class User
{
    [Key]
    public int UserId { get; set; }
    [Required]
    public string Email { get; set; }
    [Required]
    public string PasswordHash { get; set; } = string.Empty;
}
