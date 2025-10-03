using PermissionService.Core.Interfaces;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace PermissionService.Infrastructure.Services;

public class PermissionPublisher : IPermissionPublisher
{
    private readonly ISubscriber _subscriber;
    private RedisChannel KickChannel = RedisChannel.Literal("user:kick");

    public PermissionPublisher(IConnectionMultiplexer redisConnection)
    {
        _subscriber = redisConnection.GetSubscriber();
    }

    public async Task PublishKickRequestAsync(string userId, string roomId, string reason)
    {
        var message = JsonSerializer.Serialize(new { UserId = userId, Reason = reason, RoomId = roomId });
        await _subscriber.PublishAsync(KickChannel, message);
    }
}