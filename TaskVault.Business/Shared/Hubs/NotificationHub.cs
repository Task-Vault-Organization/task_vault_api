using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace TaskVault.Business.Shared.Hubs;

[Authorize]
public class NotificationHub : Hub
{
    private static readonly Dictionary<string, List<string>> Connections = new();
    
    public override Task OnConnectedAsync()
    {
        Console.WriteLine("New user connected");
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId != null)
        {
            lock (Connections)
            {
                if (!Connections.ContainsKey(userId))
                    Connections[userId] = new List<string>();

                Connections[userId].Add(Context.ConnectionId);
            }
        }

        return base.OnConnectedAsync();
    }
    
    public override Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId != null)
        {
            lock (Connections)
            {
                if (Connections.TryGetValue(userId, out var connections))
                {
                    connections.Remove(Context.ConnectionId);
                    if (connections.Count == 0)
                        Connections.Remove(userId);
                }
            }
        }

        return base.OnDisconnectedAsync(exception);
    }
    
    public static List<string> GetUserConnections(string userId)
    {
        lock (Connections)
        {
            return Connections.TryGetValue(userId, out var connections)
                ? new List<string>(connections)
                : new List<string>();
        }
    }
}