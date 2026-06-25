namespace RSS_Services;

public interface IGameHubNotifier
{
    Task NotifyTurnAdvanced(string gameId);
    Task NotifyPlayerJoined(string gameId);
    Task NotifySelectionsStarted(string gameId);
    Task NotifyGameDeleted(string gameId);
    Task NotifySquareSelected(string gameId);
    Task NotifyPlayerLeft(string gameId);
}
