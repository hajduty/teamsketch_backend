namespace PermissionService.Core.DTOs;

public class RemovePermissionRequest
{
    public string RoomId { get; set; }
    public string UserEmail { get; set; }
}