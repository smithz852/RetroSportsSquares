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
        public virtual DailySportsGames DailySportGame { get; set; }
        public string Name { get; set; }
        public string GameType { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public int PlayerCount { get; set; }
        public int PricePerSquare { get; set; }

        public AvailableGames()
        {
            Id = Guid.NewGuid();
        }
    }
    
}
