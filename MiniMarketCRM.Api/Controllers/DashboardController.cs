using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniMarketCRM.Application.DTO;
using MiniMarketCRM.DataAccess.Context;
using MiniMarketCRM.Domain.Enums;

namespace MiniMarketCRM.Api.Controllers
{
    [ApiController]
    [Route("api/dashboard")]
    public class DashboardController : ControllerBase
    {
        private readonly AppDbContext _db;
        public DashboardController(AppDbContext db) => _db = db;

        // GET /api/dashboard/summary?musteriId=5&stokEsik=5
        [HttpGet("summary")]
        public async Task<ActionResult<DashboardSummaryDTO>> Summary([FromQuery] int? musteriId, [FromQuery] int stokEsik = 5)
        {
            if (stokEsik <= 0) stokEsik = 5;

            // 1) Aktif sepet: seçili müşteri varsa ve beklemede sipariş varsa 1
            int aktifSepet = 0;
            if (musteriId.HasValue && musteriId.Value > 0)
            {
                aktifSepet = await _db.Siparisler
                    .AsNoTracking()
                    .AnyAsync(s => s.MusteriId == musteriId.Value && s.Durum == SiparisDurum.Beklemede)
                    ? 1 : 0;
            }

            // 2) Bugün satış (Tamamlandı)
            var today = DateTime.UtcNow.Date;
            var tomorrow = today.AddDays(1);

            var bugunQuery = _db.Siparisler.AsNoTracking()
                .Where(s => s.Durum == SiparisDurum.Tamamlandi
                            && s.SiparisTarihi >= today
                            && s.SiparisTarihi < tomorrow);

            var bugunSiparisAdet = await bugunQuery.CountAsync();
            var bugunCiro = await bugunQuery.SumAsync(s => (decimal?)s.ToplamTutar) ?? 0m;

            // 3) Düşük stok
            var dusukStokAdet = await _db.Urunler
                .AsNoTracking()
                .CountAsync(u => u.AktifMi && u.Stok < stokEsik);

            return new DashboardSummaryDTO
            {
                AktifSepet = aktifSepet,
                BugunSiparisAdet = bugunSiparisAdet,
                BugunCiro = bugunCiro,
                DusukStokAdet = dusukStokAdet,
                DusukStokEsik = stokEsik
            };
        }
    }
}
