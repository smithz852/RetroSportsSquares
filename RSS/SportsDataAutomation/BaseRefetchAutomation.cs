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

                var gamesInUse = GetGamesInUse();

                if (gamesInUse.Count > 0)
                {
                    foreach (var game in gamesInUse)
                    {
                        var hasGameStarted = sportsServices.HasGameStarted(game.Id);
                        if (hasGameStarted)
                        {
                            var newSportsData = await FetchSportGameData(game.Id);
                            sportsServices.UpdateSportsData(newSportsData, game.Id);
                        }
                    }
                }

                try
                {
                    await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }

        }

        protected abstract List<SportsGamesInUseDTO> GetGamesInUse();

        protected abstract Task<SportScoreUpdateDTO> FetchSportGameData(Guid gameId);
        
    }
}
