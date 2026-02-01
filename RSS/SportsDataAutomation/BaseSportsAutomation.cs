using RSS_Services;

namespace RSS.SportsDataAutomation
{
    public abstract class BaseSportsAutomation : BackgroundService
    {
        protected readonly IServiceProvider _serviceProvider;
        protected abstract string SportName { get; }
        protected abstract int LoadHourUtc { get; }

        protected BaseSportsAutomation(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (!await HasTodaysDataBeenLoaded())
            {
                await TryToLoadAvailableGames();
            }

            while (!stoppingToken.IsCancellationRequested)
            {
                var nowUtc = DateTime.UtcNow;
                var nextRun = DateTime.UtcNow.Date.AddDays(1).AddHours(LoadHourUtc);

                if (nowUtc.Hour < LoadHourUtc)
                    nextRun = DateTime.UtcNow.Date.AddHours(LoadHourUtc);

                var delay = nextRun - nowUtc;

                try
                {
                    await Task.Delay(delay, stoppingToken);
                    if (!stoppingToken.IsCancellationRequested)
                    {
                        await TryToLoadAvailableGames();
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }
        }

        protected abstract Task<bool> HasTodaysDataBeenLoaded();
        protected abstract Task TryToLoadAvailableGames();
    }
}