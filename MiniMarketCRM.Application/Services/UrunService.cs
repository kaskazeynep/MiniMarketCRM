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
    public class UrunService : IUrunService
    {
        private readonly AppDbContext _db;
        public UrunService(AppDbContext db) 
        {
            _db= db;
        }
        public async Task<List<UrunDTO>> GetAllAsync()
        {
            return await _db.Urunler
             .AsNoTracking()
             .Include(u => u.Kategori)
             .Select(u => new UrunDTO
             {
                 UrunId = u.UrunId,
                 Ad = u.Ad,
                 KategoriId = u.KategoriId,
                 KategoriAdi = u.Kategori.KategoriAdi,
                 Fiyat = u.Fiyat,
                 Stok = u.Stok,
                 AktifMi = u.AktifMi
             })
             .ToListAsync();
        }

        public async Task<UrunDTO?> GetByIdAsync(int id)
        {
            return await _db.Urunler
                .AsNoTracking()
                .Where(u => u.UrunId == id)
                .Select(u => new UrunDTO
                {
                    UrunId = u.UrunId,
                    Ad = u.Ad,
                    KategoriId = u.KategoriId,
                    Fiyat = u.Fiyat,
                    Stok = u.Stok,
                    AktifMi = u.AktifMi
                })
                .FirstOrDefaultAsync();
        }

        public async Task<int> CreateAsync(UrunUpsertDTO dto)
        {
            Validate(dto);

            // Kategori var mı?
            var kategoriVarMi = await _db.Kategoriler.AnyAsync(k => k.KategoriId == dto.KategoriId);
            if (!kategoriVarMi)
                throw new ArgumentException("Geçersiz KategoriId. Böyle bir kategori yok.");

            var entity = new Urun
            {
                Ad = dto.Ad.Trim(),
                KategoriId = dto.KategoriId,
                Fiyat = dto.Fiyat,
                Stok = dto.Stok,
                AktifMi = dto.AktifMi
            };

            _db.Urunler.Add(entity);
            await _db.SaveChangesAsync();
            return entity.UrunId;
        }

        public async Task<UrunDTO?> UpdateAsync(int id, UrunUpsertDTO dto)
        {
            Validate(dto);

            var entity = await _db.Urunler.FirstOrDefaultAsync(u => u.UrunId == id);
            if (entity == null) return null;

            // Kategori değişiyorsa kontrol et
            if (entity.KategoriId != dto.KategoriId)
            {
                var kategoriVarMi = await _db.Kategoriler.AnyAsync(k => k.KategoriId == dto.KategoriId);
                if (!kategoriVarMi)
                    throw new ArgumentException("Geçersiz KategoriId. Böyle bir kategori yok.");
            }

            entity.Ad = dto.Ad.Trim();
            entity.KategoriId = dto.KategoriId;
            entity.Fiyat = dto.Fiyat;
            entity.Stok = dto.Stok;
            entity.AktifMi = dto.AktifMi;

            await _db.SaveChangesAsync();


            return new UrunDTO
            {
                UrunId = entity.UrunId,
                Ad = entity.Ad,
                KategoriId = entity.KategoriId,
                Fiyat = entity.Fiyat,
                Stok = entity.Stok,
                AktifMi = entity.AktifMi
            };
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _db.Urunler.FirstOrDefaultAsync(u => u.UrunId == id);
            if (entity == null) return false;

            _db.Urunler.Remove(entity);

            try
            {
                await _db.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateException)
            {
                throw new ArgumentException("Bu ürün siparişlerde kullanıldığı için silinemez.");
            }
        }

        private static void Validate(UrunUpsertDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Ad))
                throw new ArgumentException("Ürün adı boş olamaz.");

            if (dto.Fiyat < 0)
                throw new ArgumentException("Fiyat negatif olamaz.");

            if (dto.Stok < 0)
                throw new ArgumentException("Stok negatif olamaz.");

            if (dto.KategoriId <= 0)
                throw new ArgumentException("KategoriId geçersiz.");
        }
    }
}
