using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MiniMarketCRM.Domain.Enums;

namespace MiniMarketCRM.Application.DTO
{
    public class SiparisUpsertDTO
    {
        public int MusteriId { get; set; }
        public DateTime SiparisTarihi { get; set; }
        public decimal ToplamTutar { get; set; }
        public SiparisDurum Durum { get; set; } = SiparisDurum.Beklemede;
    }
}
/* Dto (response için)
   UpsertDto (create + update için)
 */