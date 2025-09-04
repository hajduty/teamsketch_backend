namespace PermissionService.Core.Entities;

public class Permission
{
    public int UserId { get; set; }
    public string Room { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}