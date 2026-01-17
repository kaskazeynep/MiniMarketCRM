using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniMarketCRM.Application.DTO
{
    public class UrunUpsertDTO
    {
        [Required]
        public string Ad { get; set; } = string.Empty;

        [Range(1, int.MaxValue)]
        public int KategoriId { get; set; }

        [Range(0.01, double.MaxValue)]
        public decimal Fiyat { get; set; }

        [Range(0, int.MaxValue)]
        public int Stok { get; set; }

        public bool AktifMi { get; set; } = true;

    }
}

/* Dto (response için)
   UpsertDto (create + update için)
 */