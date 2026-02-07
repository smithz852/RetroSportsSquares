using RSS.DTOs;
using RSS_DB.Entities;

namespace RSS.Helpers
{
    public class MapperHelpers
    {
        public AvailableGamesDTO AvailableGamesMapper(SquareGames availableGames)
        {
         

            return new AvailableGamesDTO
            {
                GameId = availableGames.Id,
                GameName = availableGames.GameName,
                GameType = availableGames.GameType,
                CreatedAt = availableGames.CreatedAt,
                IsOpen = availableGames.isOpen,
                PlayerCount = availableGames.PlayerCount,
                PricePerSquare = availableGames.PricePerSquare,
                SportGameId = availableGames.DailySportGame.ApiGameId,
                HomeTeam = availableGames.DailySportGame.HomeTeam,
                AwayTeam = availableGames.DailySportGame.AwayTeam,
            };
        }

        public AvailableSportsGamesOptionsDTO AvailableSportsGamesOptionsMapper(DailySportsGames dailySportsGameOption)
        {
            return new AvailableSportsGamesOptionsDTO
            {
                Id = dailySportsGameOption.Id,
                HomeTeam = dailySportsGameOption.HomeTeam,
                AwayTeam= dailySportsGameOption.AwayTeam,
                Status = dailySportsGameOption.Status,
            };
        }
    }
}
