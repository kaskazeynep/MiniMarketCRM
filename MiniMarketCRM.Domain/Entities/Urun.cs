using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MiniMarketCRM.Domain.Entities
{
    public class Urun
    {
        [Key]
        public int UrunId { get; set; }
        public string Ad { get; set; } = string.Empty;
        public int KategoriId { get; set; }
        public decimal Fiyat { get; set; }
        public int Stok { get; set; }
        public bool AktifMi { get; set; } = true;

        public Kategori Kategori { get; set; } = null!;
        public ICollection<SiparisKalemi> SiparisKalemleri { get; set; } = new List<SiparisKalemi>();
    }
}
