namespace PermissionService.Core.Interfaces;

public interface IPermissionChannel
{
    Task NotifyPermissionChanged(int userId, string roomId, string newRole);
    Task NotifyPermissionAdded(int userId, string roomId, string role);
}