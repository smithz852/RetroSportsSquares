using RSS.DTOs;
using RSS_DB.Entities;

namespace RSS.Helpers
{
    public class MapperHelpers
    {
        public AvailableGamesDTO AvailableGamesMapper(AvailableGames availableGames)
        {
            return new AvailableGamesDTO
            {
                GameId = availableGames.GameId,
                GameName = availableGames.Name,
                GameType = availableGames.GameType,
                CreatedAt = availableGames.CreatedAt,
                Status = availableGames.Status,
                PlayerCount = availableGames.PlayerCount
            };
        }
    }
}
