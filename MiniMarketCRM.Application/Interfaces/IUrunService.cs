using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MiniMarketCRM.Application.DTO;

namespace MiniMarketCRM.Application.Interfaces
{
    public interface IUrunService
    {
        Task<List<UrunDTO>> GetAllAsync();
        Task<UrunDTO?> GetByIdAsync(int id);
        Task<int> CreateAsync(UrunUpsertDTO dto);
        Task<UrunDTO?> UpdateAsync(int id, UrunUpsertDTO dto);
        Task<bool> DeleteAsync(int id);
    }
}
