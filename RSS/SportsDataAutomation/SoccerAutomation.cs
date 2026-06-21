using RSS_Services;
using RSS_Services.Helpers;

namespace RSS.SportsDataAutomation
{
    public class SoccerAutomation : BaseSportsAutomation
    {
        protected override string SportName => "Soccer";
        protected override int LoadHourPst => 1; // 1 AM PST

        // TODO: Set LeagueId to the target league once you have the API docs
        private const string SportsType = "soccer";
        private const int LeagueId = 0; // TODO: Replace with correct league ID (e.g. MLS = ?, Premier League = 39)

        public SoccerAutomation(IServiceProvider serviceProvider) : base(serviceProvider) { }

        protected override async Task<bool> HasTodaysDataBeenLoaded()
        {
            using var scope = _serviceProvider.CreateScope();
            var soccerGameServices = scope.ServiceProvider.GetRequiredService<SportsGameServices>();
            return soccerGameServices.AreGamesInDbForToday(SportsType, LeagueId);
        }

        protected override async Task TryToLoadAvailableGames()
        {
            using var scope = _serviceProvider.CreateScope();
            var soccerGameServices = scope.ServiceProvider.GetRequiredService<SportsGameServices>();
            var timeHelpers = scope.ServiceProvider.GetRequiredService<TimeHelpers>();

            var dateString = timeHelpers.GetTimeStringTodayInPst();

            // TODO: Replace with the correct soccer API base URL and query params from the API docs
            var gameUrl = $"SOCCER_API_BASE_URL/fixtures?league={LeagueId}&date={dateString}&timezone=America%2FLos_Angeles";

            var availableGames = await soccerGameServices.GetGamesAvailableToday(SportsType, gameUrl);
            if (availableGames.Count > 0)
            {
                await soccerGameServices.SaveSportsData(availableGames);
            }
        }
    }
}
