using MiniMarketCRM.Application.DTO;

namespace MiniMarketCRM.Application.Interfaces
{
    public interface ISiparisRaporService
    {
        Task<List<SiparisRaporDTO>> GetAsync(DateTime? from = null, DateTime? to = null, int? musteriId = null);
        Task<SiparisRaporDTO?> GetByIdAsync(int siparisId);
    }
}
