using Microsoft.EntityFrameworkCore;
using MiniMarketCRM.Application.DTO;
using MiniMarketCRM.Application.Interfaces;
using MiniMarketCRM.DataAccess.Context;

namespace MiniMarketCRM.Application.Services
{
    public class SiparisRaporService : ISiparisRaporService
    {
        private readonly AppDbContext _db;

        public SiparisRaporService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<List<SiparisRaporDTO>> GetAsync(DateTime? from = null, DateTime? to = null, int? musteriId = null)
        {
            var q = _db.Siparisler
                .AsNoTracking()
                .Include(s => s.Musteri)
                .Include(s => s.SiparisKalemleri)
                    .ThenInclude(k => k.Urun)
                .AsQueryable();

            if (from.HasValue)
                q = q.Where(x => x.SiparisTarihi >= from.Value);

            if (to.HasValue)
            {
                // to gününün sonuna kadar gelsin diye:
                var toEnd = to.Value.Date.AddDays(1).AddTicks(-1);
                q = q.Where(x => x.SiparisTarihi <= toEnd);
            }

            if (musteriId.HasValue)
                q = q.Where(x => x.MusteriId == musteriId.Value);

            var list = await q
                .OrderByDescending(x => x.SiparisId)
                .ToListAsync();

            return list.Select(Map).ToList();
        }

        public async Task<SiparisRaporDTO?> GetByIdAsync(int siparisId)
        {
            var s = await _db.Siparisler
                .AsNoTracking()
                .Include(x => x.Musteri)
                .Include(x => x.SiparisKalemleri)
                    .ThenInclude(k => k.Urun)
                .FirstOrDefaultAsync(x => x.SiparisId == siparisId);

            return s == null ? null : Map(s);
        }

        private static SiparisRaporDTO Map(MiniMarketCRM.Domain.Entities.Siparis s)
        {
            return new SiparisRaporDTO
            {
                SiparisId = s.SiparisId,
                MusteriId = s.MusteriId,
                MusteriAdSoyad = s.Musteri == null ? "" : $"{s.Musteri.Ad} {s.Musteri.Soyad}",
                MusteriEmail = s.Musteri?.Email ?? "",
                SiparisTarihi = s.SiparisTarihi,
                ToplamTutar = s.ToplamTutar,
                Durum = s.Durum,
                Kalemler = s.SiparisKalemleri.Select(k => new SiparisKalemRaporDTO
                {
                    SiparisKalemiId = k.SiparisKalemiId,
                    UrunId = k.UrunId,
                    UrunAdi = k.Urun?.Ad ?? "",
                    Adet = k.Adet,
                    BirimFiyat = k.BirimFiyat,
                    SatirToplam = k.SatirToplam
                }).ToList()
            };
        }
    }
}
