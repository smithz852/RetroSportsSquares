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
        public string Name { get; set; }
        [Required]
        public string GameType { get; set; }
        [Required]
        public string Status { get; set; }
        [Required]
        public DateTime CreatedAt { get; set; }
        [Required]
        public int PlayerCount { get; set; }
        public int PricePerSquare { get; set; }
        public string? WinnerQ1Id { get; set; }
        [ForeignKey(nameof(WinnerQ1Id))]
        public ApplicationUser? WinnerQ1 { get; set; }
        public string? WinnerQ2Id { get; set; }
        [ForeignKey(nameof(WinnerQ2Id))]
        public ApplicationUser? WinnerQ2 { get; set; }
        public string? WinnerQ3Id { get; set; }
        [ForeignKey(nameof(WinnerQ3Id))]
        public ApplicationUser? WinnerQ3 { get; set; }
        public string? WinnerQ4Id { get; set; }
        [ForeignKey(nameof(WinnerQ4Id))]
        public ApplicationUser? WinnerQ4 { get; set; }
        public ICollection<GamePlayer> GamePlayers { get; set; } = new List<GamePlayer>();


        public SquareGames()
        {
            Id = Guid.NewGuid();
        }
    }
    
}
