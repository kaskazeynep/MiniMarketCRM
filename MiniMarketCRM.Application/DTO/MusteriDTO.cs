using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniMarketCRM.Application.DTO
{
    public class MusteriDTO
    {
        public int MusteriId { get; set; }
        public string Ad { get; set; } = string.Empty;
        public string Soyad { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }
}
/* Dto (response için)
   UpsertDto (create + update için)
 */