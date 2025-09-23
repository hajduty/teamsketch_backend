using System.ComponentModel.DataAnnotations;

namespace UserService.Core.Entities;

public class User
{
    [Key]
    public Guid Id { get; set; }
    [Required]
    public required string Email { get; set; }
    [Required]
    public string PasswordHash { get; set; } = string.Empty;
}
