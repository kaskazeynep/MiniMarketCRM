using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniMarketCRM.Application.DTO
{
    public class SiparisKalemiUpsertDTO
    {
        public int SiparisId { get; set; }
        public int UrunId { get; set; }
        public int Adet { get; set; }
        public decimal BirimFiyat { get; set; }
        public decimal SatirToplam { get; set; }
    }
}
/* Dto (response için)
   UpsertDto (create + update için)
 */