namespace PermissionService.Core.Entities;

public class Permission
{
    public string UserId { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
    public string Room { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}