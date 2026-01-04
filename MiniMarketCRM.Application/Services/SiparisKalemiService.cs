using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MiniMarketCRM.Application.DTO;
using MiniMarketCRM.Application.Interfaces;
using MiniMarketCRM.DataAccess.Context;
using MiniMarketCRM.Domain.Entities;

namespace MiniMarketCRM.Application.Services
{
    public class SiparisKalemiService : ISiparisKalemiService
    {
        private readonly AppDbContext _db;
        public SiparisKalemiService(AppDbContext db)
        {
            _db = db;
        }
        public async Task<List<SiparisKalemiDTO>> GetBySiparisIdAsync(int siparisId)
        {
            return await _db.SiparisKalemleri.AsNoTracking()
                .Where(k => k.SiparisId == siparisId)
                .Select(k => new SiparisKalemiDTO
                {
                    SiparisKalemiId = k.SiparisKalemiId,
                    SiparisId = k.SiparisId,
                    UrunId = k.UrunId,
                    Adet = k.Adet,
                    BirimFiyat = k.BirimFiyat,
                    SatirToplam = k.SatirToplam
                }).ToListAsync();
        }

        public async Task<SiparisKalemiDTO> AddAsync(int siparisId, SiparisKalemiUpsertDTO dto)
        {
            if (dto.Adet <= 0) throw new ArgumentException("Adet 0 veya negatif olamaz.");

            using var tx = await _db.Database.BeginTransactionAsync(); //Bu metodun içindeki birden fazla DB işlemini tek paket (tek işlem) gibi kabul edip bir yerde hata olursa hepsini geri alırırz(rollback)

            var siparis = await _db.Siparisler
                .Include(s => s.SiparisKalemleri)
                .FirstOrDefaultAsync(s => s.SiparisId == siparisId);

            if (siparis == null) throw new KeyNotFoundException("Sipariş bulunamadı.");

            var urun = await _db.Urunler.FirstOrDefaultAsync(u => u.UrunId == dto.UrunId);
            if (urun == null) throw new ArgumentException("Geçersiz UrunId.");
            if (!urun.AktifMi) throw new ArgumentException("Bu ürün pasif.");
            if (urun.Stok < dto.Adet) throw new ArgumentException("Yetersiz stok.");

            var birimFiyat = urun.Fiyat;

            var mevcut = siparis.SiparisKalemleri.FirstOrDefault(k => k.UrunId == dto.UrunId);

            if (mevcut != null)
            {
                mevcut.Adet += dto.Adet;
                mevcut.BirimFiyat = birimFiyat;
                mevcut.SatirToplam = mevcut.Adet * mevcut.BirimFiyat;
            }
            else
            {
                mevcut = new SiparisKalemi
                {
                    SiparisId = siparisId,
                    UrunId = dto.UrunId,
                    Adet = dto.Adet,
                    BirimFiyat = birimFiyat,
                    SatirToplam = dto.Adet * birimFiyat
                };
                _db.SiparisKalemleri.Add(mevcut);
                siparis.SiparisKalemleri.Add(mevcut);

            }

            urun.Stok -= dto.Adet;

            siparis.ToplamTutar = siparis.SiparisKalemleri.Sum(k => k.SatirToplam);

            await _db.SaveChangesAsync();
            await tx.CommitAsync();

            return new SiparisKalemiDTO
            {
                SiparisKalemiId = mevcut.SiparisKalemiId,
                SiparisId = mevcut.SiparisId,
                UrunId = mevcut.UrunId,
                Adet = mevcut.Adet,
                BirimFiyat = mevcut.BirimFiyat,
                SatirToplam = mevcut.SatirToplam
            };
        }

        public async Task<SiparisKalemiDTO?> UpdateAsync(int siparisId, int kalemId, SiparisKalemiUpsertDTO dto)
        {
            if (dto.Adet <= 0) throw new ArgumentException("Adet 0 veya negatif olamaz.");

            using var tx = await _db.Database.BeginTransactionAsync();

            var siparis = await _db.Siparisler
                .Include(s => s.SiparisKalemleri)
                .FirstOrDefaultAsync(s => s.SiparisId == siparisId);

            if (siparis == null) 
                return null;

            var kalem = siparis.SiparisKalemleri.FirstOrDefault(k => k.SiparisKalemiId == kalemId);
            if (kalem == null) 
                return null;

            var urun = await _db.Urunler.FirstAsync(u => u.UrunId == kalem.UrunId);

            var fark = dto.Adet - kalem.Adet; // + ise stok düş, - ise stok iade
            if (fark > 0 && urun.Stok < fark) 
                throw new ArgumentException("Yetersiz stok.");

            urun.Stok -= fark;

            kalem.Adet = dto.Adet;
            kalem.BirimFiyat = urun.Fiyat;
            kalem.SatirToplam = kalem.Adet * kalem.BirimFiyat;

            siparis.ToplamTutar = siparis.SiparisKalemleri.Sum(k => k.SatirToplam);

            await _db.SaveChangesAsync();
            await tx.CommitAsync();

            return new SiparisKalemiDTO
            {
                SiparisKalemiId = kalem.SiparisKalemiId,
                SiparisId = kalem.SiparisId,
                UrunId = kalem.UrunId,
                Adet = kalem.Adet,
                BirimFiyat = kalem.BirimFiyat,
                SatirToplam = kalem.SatirToplam
            };
        }

        public async Task<bool> DeleteAsync(int siparisId, int kalemId)
        {
            using var tx = await _db.Database.BeginTransactionAsync();

            var siparis = await _db.Siparisler
                .Include(s => s.SiparisKalemleri)
                .FirstOrDefaultAsync(s => s.SiparisId == siparisId);

            if (siparis == null) return false;

            var kalem = siparis.SiparisKalemleri.FirstOrDefault(k => k.SiparisKalemiId == kalemId);
            if (kalem == null) return false;

            var urun = await _db.Urunler.FirstAsync(u => u.UrunId == kalem.UrunId);
            urun.Stok += kalem.Adet;

            _db.SiparisKalemleri.Remove(kalem);

            siparis.ToplamTutar = siparis.SiparisKalemleri
                .Where(k => k.SiparisKalemiId != kalemId)
                .Sum(k => k.SatirToplam);

            await _db.SaveChangesAsync();
            await tx.CommitAsync();
            return true;
        }
    }
}
