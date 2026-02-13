using RSS_Services;

namespace RSS.SportsDataAutomation
{
    public abstract class BaseSportsAutomation : BackgroundService
    {
        protected readonly IServiceProvider _serviceProvider;
        protected abstract string SportName { get; }
        protected abstract int LoadHourPst { get; }

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
                var pacific = TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time");
                var nowUtc = DateTimeOffset.UtcNow;
                var nowPacific = TimeZoneInfo.ConvertTime(nowUtc, pacific);

                // Construct today's 1:00 AM *Pacific wall clock time*
                var todayRunPacific = new DateTime(
                    nowPacific.Year,
                    nowPacific.Month,
                    nowPacific.Day,
                    LoadHourPst, 0, 0,
                    DateTimeKind.Unspecified);

                var todayRunUtc = TimeZoneInfo.ConvertTimeToUtc(todayRunPacific, pacific);

                // If we've already passed it, schedule tomorrow
                if (nowUtc >= todayRunUtc)
                {
                    todayRunPacific = todayRunPacific.AddDays(1);
                    todayRunUtc = TimeZoneInfo.ConvertTimeToUtc(todayRunPacific, pacific);
                }

                var delay = todayRunUtc - nowUtc;

                if (delay <= TimeSpan.Zero)
                    delay = TimeSpan.FromMinutes(1);

                try
                {
                    await Task.Delay(delay, stoppingToken);

                    if (!stoppingToken.IsCancellationRequested)
                        await TryToLoadAvailableGames();
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