using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MiniMarketCRM.Domain.Enums;

namespace MiniMarketCRM.Domain.Entities
{
    public class Siparis
    {
        [Key]
        public int SiparisId { get; set; }
        public int MusteriId { get; set; }
        public Musteri Musteri { get; set; } = null!;
        public DateTime SiparisTarihi { get; set; } = DateTime.UtcNow;
        public decimal ToplamTutar { get; set; }
        public SiparisDurum Durum { get; set; } = SiparisDurum.Beklemede;
        public ICollection<SiparisKalemi> SiparisKalemleri { get; set; } = new List<SiparisKalemi>();
    }
}
