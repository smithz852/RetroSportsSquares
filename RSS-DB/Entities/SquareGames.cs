using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RSS_DB.Entities
{
    public class SquareGames
    {
        [Key]
        public Guid Id { get; set; }
        [Required]
        public Guid DailySportGameId { get; set; }
        [Required]
        public DailySportsGames DailySportGame { get; set; }
        [Required]
        public string GameName { get; set; }
        [Required]
        public string GameType { get; set; }
        [Required]
        public bool isOpen { get; set; }
        [Required]
        public DateTimeOffset CreatedAt { get; set; }
        [Required]
        public int PlayerCount { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal PricePerSquare { get; set; } = 0;
        public int SquareSelectionLimit { get; set; } = 0;
        public bool IsPublic { get; set; } = true;
        public bool IsTurnBased { get; set; } = false;
        public bool SelectionPhaseActive { get; set; } = false;
        public string? CurrentTurnUserId { get; set; }
        public int TurnTimeoutSeconds { get; set; } = 60;
        public DateTimeOffset? TurnStartedAt { get; set; }
        public List<int> TopNumbers { get; set; } = new List<int>();
        public List<int> LeftNumbers { get; set; } = new List<int>();
        public int PeriodCount { get; set; } = 4;
        [Required]
        [MaxLength(20)]
        public string PayoutMode { get; set; } = PayoutModes.Default;
        public bool IsCompleted { get; set; } = false;
        public bool RecapEmailSent { get; set; } = false;
        // End-of-game coin settlement ran (at-most-once guard, like RecapEmailSent)
        public bool SettlementCompleted { get; set; } = false;
        public Dictionary<int, string?> PeriodWinners { get; set; } = new();
        public ICollection<GamePlayer> GamePlayers { get; set; } = new List<GamePlayer>();
        public ICollection<GameSquares> GameSquares { get; set; } = new List<GameSquares>();

        public SquareGames()
        {
            Id = Guid.NewGuid();
        }
    }
    
}
