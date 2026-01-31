using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RSS_DB.Entities
{
    public class AvailableGames
    {
        [Key]
        public Guid Id { get; set; }
        [Required]
        public virtual DailySportsGames DailySportGame { get; set; }
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


        public AvailableGames()
        {
            Id = Guid.NewGuid();
        }
    }
    
}
