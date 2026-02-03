using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RSS_DB.Entities
{
    public class Squares
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;

        public Squares() 
        {
          Id = Guid.NewGuid();
        }

    }
}
