using Microsoft.EntityFrameworkCore;
using MiniMarketCRM.Application.DTO;
using MiniMarketCRM.Application.Interfaces;
using MiniMarketCRM.DataAccess.Context;
using MiniMarketCRM.Domain.Entities;
using MiniMarketCRM.Domain.Enums;

namespace MiniMarketCRM.Application.Services
{
    public class CartService : ICartService
    {
        private readonly AppDbContext _db;
        private readonly ISiparisKalemiService _kalemService;

        public CartService(AppDbContext db, ISiparisKalemiService kalemService)
        {
            _db = db;
            _kalemService = kalemService;
        }

        public async Task<CartDTO> GetOrCreateAsync(int musteriId)
        {
            var musteriVarMi = await _db.Musteriler.AnyAsync(m => m.MusteriId == musteriId);
            if (!musteriVarMi) throw new ArgumentException("Geçersiz MusteriId.");

            var sepet = await _db.Siparisler
                .Include(s => s.SiparisKalemleri)
                    .ThenInclude(k => k.Urun)
                .FirstOrDefaultAsync(s => s.MusteriId == musteriId && s.Durum == SiparisDurum.Beklemede);

            if (sepet is null)
                return EmptyCart(musteriId);

            var toplam = sepet.SiparisKalemleri.Sum(k => k.SatirToplam);
            if (sepet.ToplamTutar != toplam)
            {
                sepet.ToplamTutar = toplam;
                await _db.SaveChangesAsync();
            }

            return MapCart(sepet);
        }

        public async Task<CartDTO> AddItemAsync(int musteriId, CartItemAddDTO dto)
        {
            if (dto.Adet <= 0) throw new ArgumentException("Adet 0 veya negatif olamaz.");

            var musteriVarMi = await _db.Musteriler.AnyAsync(m => m.MusteriId == musteriId);
            if (!musteriVarMi) throw new ArgumentException("Geçersiz MusteriId.");

            var sepet = await _db.Siparisler
                .FirstOrDefaultAsync(s => s.MusteriId == musteriId && s.Durum == SiparisDurum.Beklemede);

            if (sepet is null)
            {
                sepet = new Siparis
                {
                    MusteriId = musteriId,
                    SiparisTarihi = DateTime.UtcNow,
                    Durum = SiparisDurum.Beklemede,
                    ToplamTutar = 0m
                };

                _db.Siparisler.Add(sepet);
                await _db.SaveChangesAsync();
            }

            await _kalemService.AddAsync(sepet.SiparisId, new SiparisKalemiUpsertDTO
            {
                SiparisId = sepet.SiparisId,
                UrunId = dto.UrunId,
                Adet = dto.Adet,
                BirimFiyat = 0,
                SatirToplam = 0
            });

            return await GetOrCreateAsync(musteriId);
        }

        public async Task<CartDTO> UpdateItemAsync(int musteriId, int kalemId, CartItemUpdateDTO dto)
        {
            if (dto.Adet <= 0) throw new ArgumentException("Adet 0 veya negatif olamaz.");

            var sepet = await _db.Siparisler
                .FirstOrDefaultAsync(s => s.MusteriId == musteriId && s.Durum == SiparisDurum.Beklemede);

            if (sepet is null) throw new KeyNotFoundException("Aktif sepet bulunamadı.");

            var updated = await _kalemService.UpdateAsync(sepet.SiparisId, kalemId, new SiparisKalemiUpsertDTO
            {
                SiparisId = sepet.SiparisId,
                Adet = dto.Adet
            });

            if (updated is null) throw new KeyNotFoundException("Kalem bulunamadı.");

            return await GetOrCreateAsync(musteriId);
        }

        public async Task<CartDTO> RemoveItemAsync(int musteriId, int kalemId)
        {
            var sepet = await _db.Siparisler
                .FirstOrDefaultAsync(s => s.MusteriId == musteriId && s.Durum == SiparisDurum.Beklemede);

            if (sepet is null) throw new KeyNotFoundException("Aktif sepet bulunamadı.");

            var ok = await _kalemService.DeleteAsync(sepet.SiparisId, kalemId);
            if (!ok) throw new KeyNotFoundException("Kalem bulunamadı.");

            return await GetOrCreateAsync(musteriId);
        }

        public async Task<SiparisDTO> CheckoutAsync(int musteriId)
        {
            await using var tx = await _db.Database.BeginTransactionAsync();

            var sepet = await _db.Siparisler
                .Include(s => s.SiparisKalemleri)
                .FirstOrDefaultAsync(s => s.MusteriId == musteriId && s.Durum == SiparisDurum.Beklemede);

            if (sepet is null) throw new KeyNotFoundException("Aktif sepet bulunamadı.");
            if (!sepet.SiparisKalemleri.Any()) throw new ArgumentException("Sepet boş. Checkout yapılamaz.");

            sepet.ToplamTutar = sepet.SiparisKalemleri.Sum(k => k.SatirToplam);
            sepet.Durum = SiparisDurum.Tamamlandi;

            await _db.SaveChangesAsync();
            await tx.CommitAsync();

            return new SiparisDTO
            {
                SiparisId = sepet.SiparisId,
                MusteriId = sepet.MusteriId,
                SiparisTarihi = sepet.SiparisTarihi,
                ToplamTutar = sepet.ToplamTutar,
                Durum = sepet.Durum
            };
        }

        public async Task<SiparisDTO> CancelAsync(int musteriId)
        {
            await using var tx = await _db.Database.BeginTransactionAsync();

            var sepet = await _db.Siparisler
                .Include(s => s.SiparisKalemleri)
                    .ThenInclude(k => k.Urun)
                .FirstOrDefaultAsync(s => s.MusteriId == musteriId && s.Durum == SiparisDurum.Beklemede);

            if (sepet is null)
                throw new KeyNotFoundException("İptal edilecek aktif sepet bulunamadı.");

            // stok iadesi
            foreach (var kalem in sepet.SiparisKalemleri)
            {
                if (kalem.Urun is not null)
                    kalem.Urun.Stok += kalem.Adet;
            }

            sepet.ToplamTutar = sepet.SiparisKalemleri.Sum(k => k.SatirToplam);
            sepet.Durum = SiparisDurum.Iptal;

            await _db.SaveChangesAsync();
            await tx.CommitAsync();

            return new SiparisDTO
            {
                SiparisId = sepet.SiparisId,
                MusteriId = sepet.MusteriId,
                SiparisTarihi = sepet.SiparisTarihi,
                ToplamTutar = sepet.ToplamTutar,
                Durum = sepet.Durum
            };
        }

        private static CartDTO MapCart(Siparis s) => new CartDTO
        {
            SiparisId = s.SiparisId,
            MusteriId = s.MusteriId,
            ToplamTutar = s.ToplamTutar,
            Items = s.SiparisKalemleri.Select(k => new CartItemDTO
            {
                SiparisKalemiId = k.SiparisKalemiId,
                UrunId = k.UrunId,
                UrunAdi = k.Urun?.Ad ?? "",
                Adet = k.Adet,
                BirimFiyat = k.BirimFiyat,
                SatirToplam = k.SatirToplam
            }).ToList()
        };

        private static CartDTO EmptyCart(int musteriId) => new CartDTO
        {
            SiparisId = 0,
            MusteriId = musteriId,
            ToplamTutar = 0m,
            Items = new List<CartItemDTO>()
        };
    }
}
