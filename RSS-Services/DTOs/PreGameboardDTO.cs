using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RSS_Services.DTOs
{
    public class PreGameboardDTO
    {
        public Guid Id { get; set; }
        public int RowIndex { get; set; }
        public int ColIndex { get; set; }
        public string? DisplayName { get; set; }
    }
}
