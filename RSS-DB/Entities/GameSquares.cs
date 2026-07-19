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
        public Guid? GamePlayerId { get; set; }
        public GamePlayer? GamePlayer { get; set; }
        // Who originally claimed (paid for) this square. Never changes — Thief
        // eliminations reassign GamePlayerId, and wagered/claimed stats must keep
        // pointing at the buyer. Null on rows claimed before this column existed
        // (readers fall back to GamePlayerId).
        public Guid? OriginalGamePlayerId { get; set; }
        public int HomeDigit { get; set; }
        public int AwayDigit { get; set; }
        public int RowIndex {  get; set; }
        public int ColumnIndex { get; set; }
        public DateTimeOffset CreatedAt { get; set; }

        public GameSquares()
        {
            Id = Guid.NewGuid();
            CreatedAt = DateTimeOffset.UtcNow;
        }
    }
}
