using System.ComponentModel.DataAnnotations;

namespace AuthService.Core.DTOs;
public class AuthRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = default!;

    [Required]
    [DataType(DataType.Password)]
    [MinLength(8)]
    public string Password { get; set; } = default!;
}