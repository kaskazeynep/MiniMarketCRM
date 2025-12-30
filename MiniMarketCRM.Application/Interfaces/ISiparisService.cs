using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MiniMarketCRM.Application.DTO;

namespace MiniMarketCRM.Application.Interfaces
{
    public interface ISiparisService
    {
        Task<List<SiparisDTO>> GetAllAsync();
        Task<SiparisDTO?> GetByIdAsync(int id);
        Task<SiparisDTO> CreateAsync(SiparisUpsertDTO dto);  
        Task<SiparisDTO?> UpdateAsync(int id, SiparisUpsertDTO dto); 
        Task<bool> DeleteAsync(int id); 
    }
}
