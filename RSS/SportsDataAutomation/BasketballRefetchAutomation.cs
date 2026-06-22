using RSS.DTOs;
using RSS_DB.Entities;
using RSS_Services;
using RSS_Services.DTOs;
using RSS_Services.Helpers;

namespace RSS.SportsDataAutomation
{
    public class BasketballRefetchAutomation : BaseRefetchAutomation
    {
        private const string SportsType = "basketball";

        // Add league IDs here to expand coverage (e.g. WNBA, EuroLeague)
        private static readonly int[] LeagueIds = { 12 }; // 12 = NBA

        public BasketballRefetchAutomation(IServiceProvider serviceProvider) : base(serviceProvider) { }

        protected override async Task<List<DailySportsGames>> GetAllGames()
        {
            using var scope = _serviceProvider.CreateScope();
            var gameServices = scope.ServiceProvider.GetRequiredService<SportsGameServices>();
            return await gameServices.GetAllGamesBySporttType(SportsType);
        }

        protected override async Task<List<SportScoreUpdateDTO>> FetchSportGameData()
        {
            using var scope = _serviceProvider.CreateScope();
            var gameServices = scope.ServiceProvider.GetRequiredService<SportsGameServices>();
            var timeHelpers = scope.ServiceProvider.GetRequiredService<TimeHelpers>();
            var todayInPst = timeHelpers.GetTimeStringTodayInPst();
            var gameUrl = $"https://v1.{SportsType}.api-sports.io/games?date={todayInPst}&timezone=America%2FLos_Angeles";

            return await gameServices.GetBasketballGameData(gameUrl, SportsType, LeagueIds);
        }
    }
}
