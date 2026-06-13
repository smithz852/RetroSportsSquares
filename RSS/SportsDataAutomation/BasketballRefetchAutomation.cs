using RSS.DTOs;
using RSS_DB.Entities;
using RSS_Services;
using RSS_Services.DTOs;
using RSS_Services.Helpers;

namespace RSS.SportsDataAutomation
{
    public class BasketballRefetchAutomation : BaseRefetchAutomation
    {
        private string sportsType = "basketball";
        public BasketballRefetchAutomation(IServiceProvider serviceProvider) : base(serviceProvider)
        {

        }

        protected override async Task<List<DailySportsGames>> GetAllGames()
        {
            var scope = _serviceProvider.CreateScope();
            var basketballGameServices = scope.ServiceProvider.GetRequiredService<SportsGameServices>();
            return await basketballGameServices.GetAllGamesBySporttType(sportsType);
        }

        protected override async Task<List<SportScoreUpdateDTO>> FetchSportGameData()
        {
            var scope = _serviceProvider.CreateScope();
            var basketballGameServices = scope.ServiceProvider.GetRequiredService<SportsGameServices>();
            var timeHelpers = scope.ServiceProvider.GetRequiredService<TimeHelpers>();
            var todayInPst = timeHelpers.GetTimeStringTodayInPst();
            var gameUrl = $"https://v1.{sportsType}.api-sports.io/games?date={todayInPst}&timezone=America%2FLos_Angeles";

            var game = await basketballGameServices.GetNbaGameData(gameUrl, sportsType);
            return game;
        }
    }
}
