using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace RSS_DB.Entities
{
    public class Squares
    {
        [Key]
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public ICollection<GamePlayerSquare> GamePlayerSquare { get; set; } = new List<GamePlayerSquare>();


        public Squares() 
        {
          Id = Guid.NewGuid();
        }

    }
}
