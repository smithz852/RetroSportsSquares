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
        public Guid GameId { get; set; }
        public string Name { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public decimal PlayerCount { get; set; }

        public AvailableGames()
        {
            GameId = Guid.NewGuid();
        }
    }
    
}
