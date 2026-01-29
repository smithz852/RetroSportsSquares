using RSS_Services;

namespace RSS.SportsDataAutomation
{
    public class NbaAutomation : BaseSportsAutomation
    {
        protected override string SportName => "NBA";
        protected override int LoadHourUtc => 9; // 2 AM PST

        public NbaAutomation(IServiceProvider serviceProvider) : base(serviceProvider) { }

        protected override async Task<bool> HasTodaysDataBeenLoaded()
        {
            using var scope = _serviceProvider.CreateScope();
            // var nbaGameServices = scope.ServiceProvider.GetRequiredService<NbaGameServices>();
            // return nbaGameServices.AreGamesInDbForToday();
            return false; // Placeholder
        }

        protected override async Task TryToLoadAvailableGames()
        {
            using var scope = _serviceProvider.CreateScope();
            // var nbaGameServices = scope.ServiceProvider.GetRequiredService<NbaGameServices>();
            // var availableGames = await nbaGameServices.GetGamesAvailableToday();
            // Save logic here
        }
    }
}