using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace MiniMarketCRM.Domain.Entities
{
    public class SiparisKalemi
    {
        [Key]
        public int SiparisKalemiId { get; set; }

        public int SiparisId { get; set; }
        public Siparis Siparis { get; set; } = null!;

        public int UrunId { get; set; }
        public Urun Urun { get; set; } = null!;

        public int Adet { get; set; }
        public decimal BirimFiyat { get; set; }
        public decimal SatirToplam { get; set; }
    }
}
