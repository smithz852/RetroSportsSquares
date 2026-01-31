using Microsoft.EntityFrameworkCore;
using RSS_DB;
using RSS_DB.Entities;
using RSS_Services.Helpers;

namespace RSS_Services
{
    public class AvailableGamesServices
    {
        private readonly AppDbContext _appDbContext;
        private readonly TimeHelpers _timeHelpers;

        public AvailableGamesServices(AppDbContext appDbContext, TimeHelpers timeHelpers)
        {
            _appDbContext = appDbContext;
            _timeHelpers = timeHelpers;
        }

        public List<AvailableGames> GetAllAvailableGames()
        {
            return _appDbContext.AvailableGames.ToList();
        }

        public AvailableGames CreateGame(string name, string status, int playerCount, string gameType, int pricePerSquare, string dailySportsGameId)
        {
            var dailySportsGameGuid = Guid.Parse(dailySportsGameId);
            var createdAt = _timeHelpers.GetTimeDateTimeTodayInPst();

            var game = new AvailableGames
            {
                Name = name,
                Status = status,
                PlayerCount = playerCount,
                CreatedAt = createdAt,
                GameType = gameType,
                PricePerSquare = pricePerSquare,
                DailySportGame = _appDbContext.DailySportsGames.FirstOrDefault(g => g.Id == dailySportsGameGuid)
            };

            return game;
        }

    }
}