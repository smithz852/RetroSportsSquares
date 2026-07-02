using RSS_Services;
using RSS_Services.Helpers;

namespace RSS.SportsDataAutomation
{
    public class SoccerAutomation : BaseSportsAutomation
    {
        protected override string SportName => "Soccer";
        protected override string SportsType => "soccer";
        protected override int LoadHourPst => 1;

        // TODO: Replace with the correct soccer API base URL from the API docs
        private const string ApiBaseUrl = "https://v3.football.api-sports.io";

        // Add league IDs here to expand coverage (e.g. MLS, Champions League)
        private static readonly int[] LeagueIds = { 1 };

        public SoccerAutomation(IServiceProvider serviceProvider) : base(serviceProvider) { }

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

           
                var gameUrl = $"{ApiBaseUrl}/fixtures?date={dateString}&timezone=America%2FLos_Angeles";
                var availableGames = await gameServices.GetGamesAvailableToday(SportsType, gameUrl, LeagueIds);
                if (availableGames.Count > 0)
                    await gameServices.SaveSportsData(availableGames);
            
        }
    }
}
