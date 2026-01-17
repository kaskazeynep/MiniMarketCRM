using System.ComponentModel.DataAnnotations;
using MiniMarketCRM.Domain.Enums;

namespace MiniMarketCRM.Application.DTO
{
    public class SiparisDurumPatchDTO
    {
        [Required]
        public SiparisDurum Durum { get; set; }
    }
}
