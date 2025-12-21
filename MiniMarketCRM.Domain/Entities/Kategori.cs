using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MiniMarketCRM.Domain.Entities
{
    public class Kategori
    {
        [Key]
        public int KategoriId { get; set; }
        public string KategoriAdi { get; set; } = string.Empty;
        public string? Aciklama { get; set; }
        // Navigation property
        public ICollection<Urun> Urunler { get; set; }= new List<Urun>();
    }
}
