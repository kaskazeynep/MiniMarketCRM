using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniMarketCRM.Application.DTO
{
    public class MusteriUpsertDTO
    {
        [Required(ErrorMessage = "Ad zorunludur.")]
        public string Ad { get; set; } = string.Empty;

        [Required(ErrorMessage = "Soyad zorunludur.")]
        public string Soyad { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email zorunludur.")]
        [EmailAddress(ErrorMessage = "Geçerli bir email adresi giriniz. (ornek@mail.com)")]
        public string Email { get; set; } = string.Empty;
    }
}
/* Dto (response için)
   UpsertDto (create + update için)
 */