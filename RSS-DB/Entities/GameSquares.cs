using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RSS_DB.Entities
{
    public class GameSquares
    {
        public Guid Id { get; set; }
        public int SquareValue { get; set; }
        public Guid SquareGamesId { get; set; }
        public SquareGames SquareGames { get; set; }
        public Guid SquaresId { get; set; }
        public Squares Squares { get; set; }
        public DateTimeOffset CreatedAt { get; set; }

        public GameSquares()
        {
            Id = Guid.NewGuid();
            CreatedAt = DateTimeOffset.UtcNow;
        }
    }
}
