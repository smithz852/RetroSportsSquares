using RSS_DB.Entities;
using RSS_Services;
using RSS_Services.DTOs;

namespace RSS.SportsDataAutomation
{
    public class NflRefetchAutomation : BaseRefetchAutomation
    {
        private string sportsType = "american-football";
        public NflRefetchAutomation(IServiceProvider serviceProvider) : base(serviceProvider)
        {

        }

        protected override List<SportsGamesInUseDTO> GetGamesInUse()
        {
            var scope = _serviceProvider.CreateScope();
            var nflGameServices = scope.ServiceProvider.GetRequiredService<SportsGameServices>();
            return nflGameServices.GetAllGamesInUse();
        }

        protected override async Task<SportScoreUpdateDTO> FetchSportGameData(Guid id)
        {
            var scope = _serviceProvider.CreateScope();
            var nflGameServices = scope.ServiceProvider.GetRequiredService<SportsGameServices>();
            var gameId = nflGameServices.GetGameApiIdFromId(id);
            var gameUrl = $"https://v1.american-football.api-sports.io/games?id={gameId}";

            var game = await nflGameServices.GetSportsGameDataByGameId(gameUrl, sportsType);
            return game;
        }
    }
}
