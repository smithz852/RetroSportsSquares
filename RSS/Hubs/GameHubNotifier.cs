using Microsoft.AspNetCore.SignalR;
using RSS_Services;

namespace RSS.Hubs;

public class GameHubNotifier : IGameHubNotifier
{
    private readonly IHubContext<GameHub> _hubContext;

    public GameHubNotifier(IHubContext<GameHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public Task NotifyTurnAdvanced(string gameId) =>
        _hubContext.Clients.Group($"game-{gameId}").SendAsync("TurnAdvanced");

    public Task NotifyPlayerJoined(string gameId) =>
        _hubContext.Clients.Group($"game-{gameId}").SendAsync("PlayerJoined");

    public Task NotifySelectionsStarted(string gameId) =>
        _hubContext.Clients.Group($"game-{gameId}").SendAsync("SelectionsStarted");
}
