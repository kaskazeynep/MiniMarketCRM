using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MiniMarketCRM.Application.DTO;

namespace MiniMarketCRM.Application.Interfaces
{
    public interface IKategoriService
    {
        Task<List<KategoriDTO>> GetAllAsync();
        Task<KategoriDTO?> GetByIdAsync(int id);
        Task<int> CreateAsync(KategoriUpsertDTO dto);
        Task<bool> UpdateAsync(int id, KategoriUpsertDTO dto);
        Task<bool> DeleteAsync(int id);
    }
}
