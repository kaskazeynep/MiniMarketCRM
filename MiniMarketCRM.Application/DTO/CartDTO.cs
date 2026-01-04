using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniMarketCRM.Application.DTO
{
    public class CartDTO
    {
        public int SiparisId { get; set; }
        public int MusteriId { get; set; }

        public decimal ToplamTutar { get; set; }

        public List<CartItemDTO> Items { get; set; } = new();
    }
}
