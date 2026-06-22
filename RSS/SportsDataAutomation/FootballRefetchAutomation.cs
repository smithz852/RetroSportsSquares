using RSS.DTOs;
using RSS_DB.Entities;
using RSS_Services;
using RSS_Services.DTOs;
using RSS_Services.Helpers;

namespace RSS.SportsDataAutomation
{
    public class FootballRefetchAutomation : BaseRefetchAutomation
    {
        private const string SportsType = "american-football";

        // TODO: Confirm base URL supports a generic sport-wide pull (no league filter in URL)
        private const string ApiBaseUrl = "https://v1.american-football.api-sports.io";

        // Add league IDs here to expand coverage (e.g. NCAA)
        private static readonly int[] LeagueIds = { 1 }; // 1 = NFL

        public FootballRefetchAutomation(IServiceProvider serviceProvider) : base(serviceProvider) { }

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
            var gameUrl = $"{ApiBaseUrl}/games?date={todayInPst}&timezone=America%2FLos_Angeles";

            return await gameServices.GetFootballGameData(gameUrl, SportsType, LeagueIds);
        }
    }
}
