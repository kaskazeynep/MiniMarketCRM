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
    public class MusteriService : IMusteriService
    {
        private readonly AppDbContext _db;
        public MusteriService(AppDbContext db)
        {
            _db = db;
        }
        public async Task<List<MusteriDTO>> GetAllAsync()
        {
            return await _db.Musteriler
                .AsNoTracking()
                .OrderByDescending(m => m.MusteriId)
                .Select(m => new MusteriDTO
                {
                    MusteriId = m.MusteriId,
                    Ad = m.Ad,
                    Soyad = m.Soyad,
                    Email = m.Email
                })
                .ToListAsync();
        }

        public async Task<MusteriDTO?> GetByIdAsync(int id)
        {
            return await _db.Musteriler
                .AsNoTracking()
                .Where(m => m.MusteriId == id)
                .Select(m => new MusteriDTO
                {
                    MusteriId = m.MusteriId,
                    Ad = m.Ad,
                    Soyad = m.Soyad,
                    Email = m.Email
                })
                .FirstOrDefaultAsync();
        }

        public async Task<MusteriDTO> CreateAsync(MusteriUpsertDTO dto)
        {
            Validate(dto);

            // Email unique kontrolü
            var normalizedEmail = dto.Email.Trim().ToLower();
            var varMi = await _db.Musteriler.AnyAsync(m => m.Email.ToLower() == normalizedEmail);
            if (varMi)
                throw new InvalidOperationException("Bu email ile kayıtlı müşteri zaten var.");

            var entity = new Musteri
            {
                Ad = dto.Ad.Trim(),
                Soyad = dto.Soyad.Trim(),
                Email = dto.Email.Trim(),
                OlusturulmaTarihi = DateTime.UtcNow
            };

            _db.Musteriler.Add(entity);
            await _db.SaveChangesAsync();

            return new MusteriDTO
            {
                MusteriId = entity.MusteriId,
                Ad = entity.Ad,
                Soyad = entity.Soyad,
                Email = entity.Email
            };
        }

        public async Task<MusteriDTO?> UpdateAsync(int id, MusteriUpsertDTO dto)
        {
            Validate(dto);

            var entity = await _db.Musteriler.FirstOrDefaultAsync(m => m.MusteriId == id);
            if (entity == null) return null;

            var normalizedEmail = dto.Email.Trim().ToLower();
            var emailBaskasindaVarMi = await _db.Musteriler
                .AnyAsync(m => m.MusteriId != id && m.Email.ToLower() == normalizedEmail);

            if (emailBaskasindaVarMi)
                throw new InvalidOperationException("Bu email başka bir müşteride kayıtlı.");

            entity.Ad = dto.Ad.Trim();
            entity.Soyad = dto.Soyad.Trim();
            entity.Email = dto.Email.Trim();

            await _db.SaveChangesAsync();

            return new MusteriDTO
            {
                MusteriId = entity.MusteriId,
                Ad = entity.Ad,
                Soyad = entity.Soyad,
                Email = entity.Email
            };
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _db.Musteriler.FirstOrDefaultAsync(m => m.MusteriId == id);
            if (entity == null) return false;

            _db.Musteriler.Remove(entity);
            await _db.SaveChangesAsync();
            return true;
        }

        private static void Validate(MusteriUpsertDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Ad))
                throw new ArgumentException("Ad boş olamaz.");

            if (string.IsNullOrWhiteSpace(dto.Soyad))
                throw new ArgumentException("Soyad boş olamaz.");

            if (string.IsNullOrWhiteSpace(dto.Email))
                throw new ArgumentException("Email boş olamaz.");
        }
    }
}
