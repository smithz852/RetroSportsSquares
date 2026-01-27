namespace RSS.SportsDataAutomation
{
    public class FootballAutomation : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Check if we need to run immediately on startup
            var now = DateTime.Now;
            if (now.Hour >= 1 && !HasTodaysDataBeenLoaded())
            {
                await DoYourWork();
            }

            while (!stoppingToken.IsCancellationRequested)
            {
                now = DateTime.Now;
                var nextRun = DateTime.Today.AddDays(1).AddHours(1); // Tomorrow at 1 AM

                if (now.Hour < 1) // If it's before 1 AM today
                    nextRun = DateTime.Today.AddHours(1); // Run today at 1 AM

                var delay = nextRun - now;

                try
                {
                    await Task.Delay(delay, stoppingToken);

                    if (!stoppingToken.IsCancellationRequested)
                    {
                        await DoYourWork();
                    }
                }
                catch (OperationCanceledException)
                {
                    break; // Expected when shutting down
                }
            }
        }

        private bool HasTodaysDataBeenLoaded()
        {
            // Check if today's data has already been loaded
            return false; // Implement your logic here
        }

        private async Task DoYourWork()
        {
            // Your data loading logic here
        }
    }
}
