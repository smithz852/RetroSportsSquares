using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RSS_DB.Entities
{
    public class GamePlayer
    {
        public Guid Id { get; set; }
        [Required]
        public Guid GameId { get; set; }
        public SquareGames Game { get; set; }
        [Required]
        public string ApplicationUserId { get; set; }
        public ApplicationUser User { get; set; }
        public int TurnOrder {  get; set; }
        public bool IsHost {  get; set; } = false;
        public int NumbersOfSquareSelected { get; set; } = 0;
        public decimal TotalWagerAmount { get; set; } = 0;
        public ICollection<GamePlayerSquare> GamePlayerSquares { get; set; } = new List<GamePlayerSquare>();
    }
}
