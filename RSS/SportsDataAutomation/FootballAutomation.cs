using RSS_Services;

namespace RSS.SportsDataAutomation
{
    public class FootballAutomation : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;

        public FootballAutomation(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Check if we need to run immediately on startup
            var nowUtc = DateTime.UtcNow;
            if (!await HasTodaysDataBeenLoaded())
            {
                await DoYourWork(); //need to add function for pull game data from api, and saving it to db
            }

            while (!stoppingToken.IsCancellationRequested)
            {
                nowUtc = DateTime.UtcNow;
                var nextRun = DateTime.UtcNow.Date.AddDays(1).AddHours(9); // Tomorrow at 9 AM UTC (1 AM PST)

                if (nowUtc.Hour < 9) // If it's before 9 AM UTC today
                    nextRun = DateTime.UtcNow.Date.AddHours(9); // Run today at 9 AM UTC

                var delay = nextRun - nowUtc;

                try
                {
                    await Task.Delay(delay, stoppingToken);

                    if (!stoppingToken.IsCancellationRequested)
                    {
                        //await DoYourWork();
                    }
                }
                catch (OperationCanceledException)
                {
                    break; // Expected when shutting down
                }
            }
        }

        private async Task<bool> HasTodaysDataBeenLoaded()
        {
            using var scope = _serviceProvider.CreateScope();
            var nflGameServices = scope.ServiceProvider.GetRequiredService<NflGameServices>();
            
            var gamesToday = nflGameServices.AreGamesInDbForToday();
            if (!gamesToday)
            {
                var availableGames = await nflGameServices.AreNflGamesAvailableToday();
                if (!availableGames)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            return true;
        }

        private async Task DoYourWork()
        {
            var two = 1 + 1;
        }
    }
}
