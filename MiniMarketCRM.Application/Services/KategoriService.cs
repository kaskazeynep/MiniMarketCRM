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
    public class KategoriService : IKategoriService
    {
        private readonly AppDbContext _db;

        public KategoriService(AppDbContext db) 
        {
            _db = db;
        }
        public async Task<List<KategoriDTO>> GetAllAsync()
        {
            return await _db.Kategoriler
                .AsNoTracking()
                .Select(k => new KategoriDTO
                {
                    KategoriId = k.KategoriId,
                    KategoriAdi = k.KategoriAdi,
                    Aciklama = k.Aciklama
                })
                .ToListAsync();
        }

        public async Task<KategoriDTO?> GetByIdAsync(int id)
        {
            return await _db.Kategoriler
                .AsNoTracking()
                .Where(k => k.KategoriId == id)
                .Select(k => new KategoriDTO
                {
                    KategoriId = k.KategoriId,
                    KategoriAdi = k.KategoriAdi,
                    Aciklama = k.Aciklama
                })
                .FirstOrDefaultAsync();
        }

        public async Task<int> CreateAsync(KategoriUpsertDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto.KategoriAdi))
                throw new ArgumentException("Kategori adı boş olamaz.");

            var entity = new Kategori
            {
                KategoriAdi = dto.KategoriAdi.Trim(),
                Aciklama = dto.Aciklama
            };

            _db.Kategoriler.Add(entity);
            await _db.SaveChangesAsync();

            return entity.KategoriId;
        }

        public async Task<bool> UpdateAsync(int id, KategoriUpsertDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto.KategoriAdi))
                throw new ArgumentException("Kategori adı boş olamaz.");

            var entity = await _db.Kategoriler.FirstOrDefaultAsync(k => k.KategoriId == id);
            if (entity is null) return false;

            entity.KategoriAdi = dto.KategoriAdi.Trim();
            entity.Aciklama = dto.Aciklama;

            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _db.Kategoriler.FirstOrDefaultAsync(k => k.KategoriId == id);
            if (entity is null) return false;

            _db.Kategoriler.Remove(entity);
            await _db.SaveChangesAsync();
            return true;
        }
    }
}
