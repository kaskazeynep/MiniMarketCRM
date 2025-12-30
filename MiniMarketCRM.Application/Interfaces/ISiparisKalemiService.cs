using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MiniMarketCRM.Application.DTO;

namespace MiniMarketCRM.Application.Interfaces
{
    public interface ISiparisKalemiService
    {
        Task<List<SiparisKalemiDTO>> GetBySiparisIdAsync(int siparisId);

        Task<SiparisKalemiDTO> AddAsync(int siparisId, SiparisKalemiUpsertDTO dto); // aynı ürün varsa artır
        Task<SiparisKalemiDTO?> UpdateAsync(int siparisId, int kalemId, SiparisKalemiUpsertDTO dto); // adet değiştir
        Task<bool> DeleteAsync(int siparisId, int kalemId); // stok iade + toplam güncelle
    }
}
