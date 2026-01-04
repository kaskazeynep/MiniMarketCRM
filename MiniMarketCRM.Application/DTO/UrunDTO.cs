using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniMarketCRM.Application.DTO
{
    public class UrunDTO
    {
        public int UrunId { get; set; }
        public string Ad { get; set; } = string.Empty;
        public int KategoriId { get; set; }
        public string KategoriAdi { get; set; } = string.Empty;
        public decimal Fiyat { get; set; }
        public int Stok { get; set; }
        public bool AktifMi { get; set; } 
    }
}
/* Dto (response için)
   UpsertDto (create + update için)
 */