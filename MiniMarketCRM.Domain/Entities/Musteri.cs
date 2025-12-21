using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MiniMarketCRM.Domain.Entities
{
    public class Musteri
    {
        [Key]
        public int MusteriId { get; set; }
        public string Ad { get; set; } =string.Empty;
        public string Soyad { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime OlusturulmaTarihi { get; set; } = DateTime.UtcNow;
        public ICollection<Siparis> Siparisler { get; set; } = new List<Siparis>();
    }
}
