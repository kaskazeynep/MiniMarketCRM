using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniMarketCRM.Application.DTO
{
    public class KategoriUpsertDTO
    {
        public string KategoriAdi { get; set; } = string.Empty;
        public string? Aciklama { get; set; }
    }
}
/* Dto (response için)
   UpsertDto (create + update için)
 */