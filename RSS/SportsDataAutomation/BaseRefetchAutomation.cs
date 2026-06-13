using RSS.DTOs;
using RSS_DB.Entities;
using RSS_Services;
using RSS_Services.DTOs;
using RSS_Services.Helpers;

namespace RSS.SportsDataAutomation
{
    public abstract class BaseRefetchAutomation : BackgroundService
    {
        protected readonly IServiceProvider _serviceProvider;

        protected BaseRefetchAutomation(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using var scope = _serviceProvider.CreateScope();
                var sportsServices = scope.ServiceProvider.GetRequiredService<SportsGameServices>();
                var squareServices = scope.ServiceProvider.GetRequiredService<SquareServices>();

                var getAllGames = await GetAllGames();

                if (getAllGames.Count > 0)
                {
                    var newSportsData = await FetchSportGameData();
                    await sportsServices.UpdateSportsDataAsync(newSportsData);

                    foreach (var game in getAllGames)
                    {
                        if (!sportsServices.HasGameStarted(game.Id)) continue;

                        var gameData = newSportsData.FirstOrDefault(d => d.ApiGameId == game.ApiGameId);
                        if (gameData == null) continue;

                        if (gameData.Status is "FT" or "AOT" or "Final/OT" or "Postponed") continue;

                        await ProcessQuarterlyWinners(squareServices, gameData, game.Id);
                    }
                }

                try
                {
                    await Task.Delay(TimeSpan.FromMinutes(7), stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }
        }

        private async Task ProcessQuarterlyWinners(SquareServices squareServices, SportScoreUpdateDTO newSportsData, Guid sportGameId)
        {
            var squareGames = await squareServices.GetSquareGamesBySportsGameId(sportGameId);
            foreach (var squareGame in squareGames)
            {
                var winner = await squareServices.DetermineQuarterlyWinner(newSportsData, squareGame.Id);
                if (winner?.UserId != null)
                    await squareServices.SaveQuarterlyWinner(winner, squareGame.Id);
            }
        }

        protected abstract Task<List<DailySportsGames>> GetAllGames();
        protected abstract Task<List<SportScoreUpdateDTO>> FetchSportGameData();
    }
}
