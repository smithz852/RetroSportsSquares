using RSS_Services;
using RSS_Services.Helpers;

namespace RSS.SportsDataAutomation
{
    public class FootballAutomation : BaseSportsAutomation
    {
        protected override string SportName => "Football";
        protected override int LoadHourPst => 1;

        private const string SportsType = "american-football";

        // TODO: Confirm the correct base URL for a generic american-football data pull
        // The current URL targets api-sports.io — swap if you move to a different provider
        private const string ApiBaseUrl = "https://v1.american-football.api-sports.io";

        // Add league IDs here to expand coverage (e.g. NCAA)
        private static readonly int[] LeagueIds = { 1 }; // 1 = NFL

        public FootballAutomation(IServiceProvider serviceProvider) : base(serviceProvider) { }

        protected override async Task<bool> HasTodaysDataBeenLoaded()
        {
            using var scope = _serviceProvider.CreateScope();
            var gameServices = scope.ServiceProvider.GetRequiredService<SportsGameServices>();
            return LeagueIds.All(id => gameServices.AreGamesInDbForToday(SportsType, id));
        }

        protected override async Task TryToLoadAvailableGames()
        {
            using var scope = _serviceProvider.CreateScope();
            var gameServices = scope.ServiceProvider.GetRequiredService<SportsGameServices>();
            var timeHelpers = scope.ServiceProvider.GetRequiredService<TimeHelpers>();
            var dateString = timeHelpers.GetTimeStringTodayInPst();



            var gameUrl = $"{ApiBaseUrl}/games?date={dateString}&timezone=America%2FLos_Angeles";
                var availableGames = await gameServices.GetGamesAvailableToday(SportsType, gameUrl, LeagueIds);
                if (availableGames.Count > 0)
                    await gameServices.SaveSportsData(availableGames);
            
        }
    }
}
