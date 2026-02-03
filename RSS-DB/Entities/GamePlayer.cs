using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RSS_DB.Entities
{
    public class GamePlayer
    {
        public Guid Id { get; set; }
        public Guid GameId { get; set; }
        public SquareGames Game { get; set; }
        public Guid ApplicationUserId { get; set; }
        public ApplicationUser User { get; set; }
        public int TurnOrder {  get; set; }
        public bool IsHost {  get; set; }
        public int NumbersOfSquareSelected {  get; set; }
        public decimal TotalWagerAmount { get; set; }
        public ICollection<GamePlayerSquare> GamePlayerSquares { get; set; } = new List<GamePlayerSquare>();
    }
}
