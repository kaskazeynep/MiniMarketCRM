using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniMarketCRM.Application.DTO
{
    public class CartItemDTO
    {
        public int SiparisKalemiId { get; set; }
        public int UrunId { get; set; }
        public string UrunAdi { get; set; } = string.Empty;

        public int Adet { get; set; }
        public decimal BirimFiyat { get; set; }
        public decimal SatirToplam { get; set; }
    }
}
