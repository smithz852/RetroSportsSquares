using RSS.DTOs;
using RSS_Services;
using RSS_Services.DTOs;

namespace RSS.SportsDataAutomation
{
    public class BasketballRefetchAutomation : BaseRefetchAutomation
    {
        private string sportsType = "basketball";
        public BasketballRefetchAutomation(IServiceProvider serviceProvider) : base(serviceProvider)
        {

        }

        protected override List<SportsGamesInUseDTO> GetGamesInUse()
        {
            var scope = _serviceProvider.CreateScope();
            var basketballGameServices = scope.ServiceProvider.GetRequiredService<SportsGameServices>();
            return basketballGameServices.GetAllGamesInUse(sportsType);
        }

        protected override async Task<SportScoreUpdateDTO> FetchSportGameData(Guid id)
        {
            var scope = _serviceProvider.CreateScope();
            var basketballGameServices = scope.ServiceProvider.GetRequiredService<SportsGameServices>();
            var gameId = basketballGameServices.GetGameApiIdFromId(id);
            var gameUrl = $"https://v1.{sportsType}.api-sports.io/games?id={gameId}&timezone=America%2FLos_Angeles";

            var game = await basketballGameServices.GetSportsGameDataByGameId(gameUrl, sportsType);
            return game;
        }
    }
}
