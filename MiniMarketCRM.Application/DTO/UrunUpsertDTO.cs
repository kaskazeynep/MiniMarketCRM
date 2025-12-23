using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniMarketCRM.Application.DTO
{
    public class UrunUpsertDTO
    {
        public string Ad { get; set; } = string.Empty;
        public int KategoriId { get; set; }
        public decimal Fiyat { get; set; }
        public int Stok { get; set; }
        public bool AktifMi { get; set; }

    }
}

/* Dto (response için)
   UpsertDto (create + update için)
 */