using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MiniMarketCRM.Application.DTO;
using MiniMarketCRM.Domain.Enums;

namespace MiniMarketCRM.Application.Interfaces
{
    public interface ISiparisService
    {
        Task<List<SiparisDTO>> GetAllAsync();
        Task<SiparisDTO?> GetByIdAsync(int id);
        Task<SiparisDTO> CreateAsync(SiparisUpsertDTO dto);  
        Task<SiparisDTO?> UpdateAsync(int id, SiparisUpsertDTO dto); 
        Task<bool> DeleteAsync(int id);
      
        Task<SiparisDTO?> UpdateDurumAsync(int id, SiparisDurum durum);
        Task<List<SiparisRaporDTO>> GetRaporAsync(DateTime? from, DateTime? to);
        Task<SiparisRaporDTO?> GetRaporDetayAsync(int siparisId);

    }
}
