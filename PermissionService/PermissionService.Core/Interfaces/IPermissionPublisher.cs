using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PermissionService.Core.Interfaces;

public interface IPermissionPublisher
{
    Task PublishKickRequestAsync(string userId, string roomId, string reason);
}
