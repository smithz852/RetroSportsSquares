using RSS_DB;
using RSS_DB.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RSS_Services
{
    public class SquareServices
    {
        private AppDbContext _appDbContext;
        public SquareServices(AppDbContext appDbContext) 
        { 
            _appDbContext = appDbContext;
        }

        public List<GamePlayerSquare> CreateSquareSelections(List<string> squareSelections, string userId)
        {
            var gamePlayerSquares = new List<GamePlayerSquare>();
            var userIdGuid = Guid.Parse(userId);
            var createdAt = DateTimeOffset.UtcNow;

            foreach (var square in squareSelections)
            {
                var selectedSquare = _appDbContext.Squares.FirstOrDefault(s => s.Name == square);
                GamePlayerSquare gamePlayerSquare = new GamePlayerSquare()
                {
                    SelectedAt = createdAt,
                    GamePlayerId = userIdGuid,
                    SquareId = selectedSquare.Id
                };
                gamePlayerSquares.Add(gamePlayerSquare);
            }
            return gamePlayerSquares;
        }
    }
}
