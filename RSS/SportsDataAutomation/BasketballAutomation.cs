using RSS_Services;
using RSS_Services.Helpers;

namespace RSS.SportsDataAutomation
{
    public class BasketballAutomation : BaseSportsAutomation
    {
        protected override string SportName => "Basketball";
        protected override string SportsType => "basketball";
        protected override int LoadHourPst => 1;

        // Add league IDs here to expand coverage (e.g. WNBA, EuroLeague)
        private static readonly int[] LeagueIds = { 12 };

        public BasketballAutomation(IServiceProvider serviceProvider) : base(serviceProvider) { }

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

                var gameUrl = $"https://v1.{SportsType}.api-sports.io/games?date={dateString}&timezone=America%2FLos_Angeles";
            var availableGames = await gameServices.GetGamesAvailableToday(SportsType, gameUrl, LeagueIds);
                if (availableGames.Count > 0)
                    await gameServices.SaveSportsData(availableGames);
          
        }
    }
}
