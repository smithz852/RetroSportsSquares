using RSS.DTOs;
using RSS_DB.Entities;
using RSS_Services;
using RSS_Services.DTOs;
using RSS_Services.Helpers;

namespace RSS.SportsDataAutomation
{
    public class SoccerRefetchAutomation : BaseRefetchAutomation
    {
        private const string SportsType = "soccer";

        // TODO: Replace with the correct soccer API base URL from the API docs
        private const string ApiBaseUrl = "https://v3.football.api-sports.io";

        // Add league IDs here to expand coverage (e.g. MLS, Champions League)
        private static readonly int[] LeagueIds = { 1 }; 

        public SoccerRefetchAutomation(IServiceProvider serviceProvider) : base(serviceProvider) { }

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

            var gameUrl = $"{ApiBaseUrl}/fixtures?date={todayInPst}&timezone=America%2FLos_Angeles";

            return await gameServices.GetSoccerGameData(gameUrl, SportsType, LeagueIds);
        }
    }
}
