using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PermissionService.Core.DTOs;

public class PermissionRequest
{
    public required string UserId { get; set; } = string.Empty;
    public required string Room { get; set; } = string.Empty;
    public required string Role { get; set; } = string.Empty;
    public required string UserEmail { get; set; } = string.Empty;
}