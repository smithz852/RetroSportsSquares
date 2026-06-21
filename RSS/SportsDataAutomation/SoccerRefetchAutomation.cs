using RSS.DTOs;
using RSS_DB.Entities;
using RSS_Services;
using RSS_Services.Helpers;

namespace RSS.SportsDataAutomation
{
    public class SoccerRefetchAutomation : BaseRefetchAutomation
    {
        // TODO: Replace with the correct soccer API base URL from the API docs
        private const string SportsType = "soccer";

        public SoccerRefetchAutomation(IServiceProvider serviceProvider) : base(serviceProvider) { }

        protected override async Task<List<DailySportsGames>> GetAllGames()
        {
            var scope = _serviceProvider.CreateScope();
            var soccerGameServices = scope.ServiceProvider.GetRequiredService<SportsGameServices>();
            return await soccerGameServices.GetAllGamesBySporttType(SportsType);
        }

        protected override async Task<List<SportScoreUpdateDTO>> FetchSportGameData()
        {
            var scope = _serviceProvider.CreateScope();
            var soccerGameServices = scope.ServiceProvider.GetRequiredService<SportsGameServices>();
            var timeHelpers = scope.ServiceProvider.GetRequiredService<TimeHelpers>();

            var todayInPst = timeHelpers.GetTimeStringTodayInPst();

            // TODO: Replace with the correct soccer API base URL and query params from the API docs
            var gameUrl = $"SOCCER_API_BASE_URL/fixtures?date={todayInPst}&timezone=America%2FLos_Angeles&live=all";

            return await soccerGameServices.GetSoccerGameData(gameUrl, SportsType);
        }
    }
}
