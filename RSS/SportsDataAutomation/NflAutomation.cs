using RSS_Services;

namespace RSS.SportsDataAutomation
{
    public class NflAutomation : BaseSportsAutomation
    {
        protected override string SportName => "NFL";
        protected override int LoadHourUtc => 9; // 1 AM PST
        private string SportsType = "american-football";
        private int LeagueId = 1;

        public NflAutomation(IServiceProvider serviceProvider) : base(serviceProvider) { }

        protected override async Task<bool> HasTodaysDataBeenLoaded()
        {
            using var scope = _serviceProvider.CreateScope();
            var nflGameServices = scope.ServiceProvider.GetRequiredService<SportsGameServices>();
            return nflGameServices.AreGamesInDbForToday(SportsType, LeagueId);
        }

        protected override async Task TryToLoadAvailableGames()
        {
            using var scope = _serviceProvider.CreateScope();
            var nflGameServices = scope.ServiceProvider.GetRequiredService<SportsGameServices>();
            
            var availableGames = await nflGameServices.GetGamesAvailableToday(SportsType, LeagueId);
            if (availableGames.Count > 0)
            {
                //save to db here
            }
            
        }
    }
}