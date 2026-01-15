using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MiniMarketCRM.Domain.Enums;

namespace MiniMarketCRM.Application.DTO
{
    public class SiparisRaporDTO
    {
        public int SiparisId { get; set; }
        public DateTime SiparisTarihi { get; set; }
        public decimal ToplamTutar { get; set; }
        public SiparisDurum Durum { get; set; } 
        public int MusteriId { get; set; }
        public string MusteriAdSoyad { get; set; } = "";
        public string MusteriEmail { get; set; } = "";

        public List<SiparisKalemRaporDTO> Kalemler { get; set; } = new();
    }
}
