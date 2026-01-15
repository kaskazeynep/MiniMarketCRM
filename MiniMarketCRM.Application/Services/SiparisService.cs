using Microsoft.EntityFrameworkCore;
using MiniMarketCRM.Application.DTO;
using MiniMarketCRM.Application.Interfaces;
using MiniMarketCRM.DataAccess.Context;
using MiniMarketCRM.Domain.Entities;

namespace MiniMarketCRM.Application.Services
{
    public class SiparisService : ISiparisService
    {
        private readonly AppDbContext _db;

        public SiparisService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<List<SiparisDTO>> GetAllAsync()
        {
            return await _db.Siparisler
                .AsNoTracking()
                .OrderByDescending(s => s.SiparisId)
                .Select(s => new SiparisDTO
                {
                    SiparisId = s.SiparisId,
                    MusteriId = s.MusteriId,
                    SiparisTarihi = s.SiparisTarihi,
                    ToplamTutar = s.ToplamTutar,
                    Durum = s.Durum
                })
                .ToListAsync();
        }

        public async Task<SiparisDTO?> GetByIdAsync(int id)
        {
            return await _db.Siparisler
                .AsNoTracking()
                .Where(s => s.SiparisId == id)
                .Select(s => new SiparisDTO
                {
                    SiparisId = s.SiparisId,
                    MusteriId = s.MusteriId,
                    SiparisTarihi = s.SiparisTarihi,
                    ToplamTutar = s.ToplamTutar,
                    Durum = s.Durum
                })
                .FirstOrDefaultAsync();
        }

        public async Task<SiparisDTO> CreateAsync(SiparisUpsertDTO dto)
        {
            // Musteri var mı?
            var musteriVarMi = await _db.Musteriler.AnyAsync(m => m.MusteriId == dto.MusteriId);
            if (!musteriVarMi) throw new ArgumentException("Geçersiz MusteriId.");

            var entity = new Siparis
            {
                MusteriId = dto.MusteriId,
                SiparisTarihi = dto.SiparisTarihi,
                Durum = dto.Durum,
                ToplamTutar = 0m // kalemlerden hesaplanacak
            };

            _db.Siparisler.Add(entity);
            await _db.SaveChangesAsync();

            return new SiparisDTO
            {
                SiparisId = entity.SiparisId,
                MusteriId = entity.MusteriId,
                SiparisTarihi = entity.SiparisTarihi,
                ToplamTutar = entity.ToplamTutar,
                Durum = entity.Durum
            };
        }

        public async Task<SiparisDTO?> UpdateAsync(int id, SiparisUpsertDTO dto)
        {
            var entity = await _db.Siparisler.FirstOrDefaultAsync(s => s.SiparisId == id);
            if (entity == null) return null;

            // musteri değişiyorsa kontrol
            if (entity.MusteriId != dto.MusteriId)
            {
                var musteriVarMi = await _db.Musteriler.AnyAsync(m => m.MusteriId == dto.MusteriId);
                if (!musteriVarMi) throw new ArgumentException("Geçersiz MusteriId.");

                entity.MusteriId = dto.MusteriId;
            }

            entity.SiparisTarihi = dto.SiparisTarihi;
            entity.Durum = dto.Durum;

            // ToplamTutar: kalemlerden hesapla (client’tan geleni yok say)
            entity.ToplamTutar = await _db.SiparisKalemleri
                .Where(sk => sk.SiparisId == id)
                .SumAsync(sk => (decimal?)sk.SatirToplam) ?? 0m;

            await _db.SaveChangesAsync();

            return new SiparisDTO
            {
                SiparisId = entity.SiparisId,
                MusteriId = entity.MusteriId,
                SiparisTarihi = entity.SiparisTarihi,
                ToplamTutar = entity.ToplamTutar,
                Durum = entity.Durum
            };
        }

        public async Task<bool> DeleteAsync(int id)
        {
            // Siparişi kalemleriyle çekme (stok iadesi için)
            var siparis = await _db.Siparisler
                .Include(s => s.SiparisKalemleri)
                .FirstOrDefaultAsync(s => s.SiparisId == id);

            if (siparis == null) return false;

            // stok iadesi
            var urunIds = siparis.SiparisKalemleri.Select(k => k.UrunId).Distinct().ToList();
            var urunler = await _db.Urunler.Where(u => urunIds.Contains(u.UrunId)).ToListAsync();

            foreach (var kalem in siparis.SiparisKalemleri)
            {
                var urun = urunler.First(u => u.UrunId == kalem.UrunId);
                urun.Stok += kalem.Adet;
            }

            _db.Siparisler.Remove(siparis);
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<List<SiparisRaporDTO>> GetRaporAsync(DateTime? from, DateTime? to)
        {
            var q = _db.Siparisler
                .AsNoTracking()
                .Include(s => s.Musteri)
                .Include(s => s.SiparisKalemleri)
                    .ThenInclude(k => k.Urun)
                .AsQueryable();

            if (from.HasValue) q = q.Where(x => x.SiparisTarihi >= from.Value);
            if (to.HasValue) q = q.Where(x => x.SiparisTarihi <= to.Value);

            return await q.OrderByDescending(x => x.SiparisId)
                .Select(s => new SiparisRaporDTO
                {
                    SiparisId = s.SiparisId,
                    SiparisTarihi = s.SiparisTarihi,
                    ToplamTutar = s.ToplamTutar,
                    Durum = s.Durum,
                    MusteriId = s.MusteriId,
                    MusteriAdSoyad = (s.Musteri.Ad + " " + s.Musteri.Soyad),
                    MusteriEmail = s.Musteri.Email,
                    Kalemler = s.SiparisKalemleri.Select(k => new SiparisKalemRaporDTO
                    {
                        SiparisKalemiId = k.SiparisKalemiId,
                        UrunId = k.UrunId,
                        UrunAdi = k.Urun.Ad,
                        Adet = k.Adet,
                        BirimFiyat = k.BirimFiyat,
                        SatirToplam = k.SatirToplam
                    }).ToList()
                })
                .ToListAsync();
        }

        public async Task<SiparisRaporDTO?> GetRaporDetayAsync(int siparisId)
        {
            return await _db.Siparisler
                .AsNoTracking()
                .Include(s => s.Musteri)
                .Include(s => s.SiparisKalemleri)
                    .ThenInclude(k => k.Urun)
                .Where(s => s.SiparisId == siparisId)
                .Select(s => new SiparisRaporDTO
                {
                    SiparisId = s.SiparisId,
                    SiparisTarihi = s.SiparisTarihi,
                    ToplamTutar = s.ToplamTutar,
                    Durum = s.Durum,
                    MusteriId = s.MusteriId,
                    MusteriAdSoyad = (s.Musteri.Ad + " " + s.Musteri.Soyad),
                    MusteriEmail = s.Musteri.Email,
                    Kalemler = s.SiparisKalemleri.Select(k => new SiparisKalemRaporDTO
                    {
                        SiparisKalemiId = k.SiparisKalemiId,
                        UrunId = k.UrunId,
                        UrunAdi = k.Urun.Ad,
                        Adet = k.Adet,
                        BirimFiyat = k.BirimFiyat,
                        SatirToplam = k.SatirToplam
                    }).ToList()
                })
                .FirstOrDefaultAsync();
        }

    }
}
