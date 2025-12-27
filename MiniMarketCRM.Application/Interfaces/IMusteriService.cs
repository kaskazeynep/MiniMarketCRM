using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MiniMarketCRM.Application.DTO;

namespace MiniMarketCRM.Application.Interfaces
{
    public interface IMusteriService
    {
        Task<List<MusteriDTO>> GetAllAsync();
        Task<MusteriDTO?> GetByIdAsync(int id);
        Task<MusteriDTO> CreateAsync(MusteriUpsertDTO dto);
        Task<MusteriDTO?> UpdateAsync(int id, MusteriUpsertDTO dto);
        Task<bool> DeleteAsync(int id);
    }
}
