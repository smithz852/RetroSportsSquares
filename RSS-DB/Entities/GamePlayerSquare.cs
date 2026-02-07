using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RSS_DB.Entities
{
    public class GamePlayerSquare
    {
        [Key]
        public Guid Id { get; set; }
        public Guid GamePlayerId { get; set; }
        public GamePlayer GamePlayer { get; set; }
        public Guid SquareId { get; set; }
        public Squares Squares { get; set; }
    }
}
