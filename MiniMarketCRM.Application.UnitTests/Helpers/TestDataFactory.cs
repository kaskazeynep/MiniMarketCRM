using Microsoft.EntityFrameworkCore;
using MiniMarketCRM.DataAccess.Context;
using MiniMarketCRM.Domain.Entities;
using MiniMarketCRM.Domain.Enums;

namespace MiniMarketCRM.Application.UnitTests.Helpers;

public static class TestDataFactory
{
    public static AppDbContext CreateDbContext(string? dbName = null)
    {
        dbName ??= Guid.NewGuid().ToString();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(dbName)
            .EnableSensitiveDataLogging()
            .Options;

        return new AppDbContext(options);
    }

    public static async Task<int> SeedMusteriAsync(AppDbContext db, int musteriId = 1)
    {
        db.Musteriler.Add(new MiniMarketCRM.Domain.Entities.Musteri
        {
            MusteriId = musteriId,
            Ad = "Zeynep",
            Soyad = "Kaska",
            Email = $"zeynep{musteriId}@test.com",
            OlusturulmaTarihi = DateTime.UtcNow
        });

        await db.SaveChangesAsync();
        return musteriId;
    }

    public static async Task<Urun> SeedUrunAsync(
        AppDbContext db,
        int urunId = 1,
        int stok = 100,
        bool aktifMi = true,
        decimal fiyat = 10m)
    {
        var urun = new Urun
        {
            UrunId = urunId,
            Ad = "Test Ürün",
            Stok = stok,
            AktifMi = aktifMi,
            Fiyat = fiyat
        };

        db.Urunler.Add(urun);
        await db.SaveChangesAsync();
        return urun;
    }

    public static async Task<Siparis> SeedBeklemedeSepetAsync(AppDbContext db, int musteriId, int siparisId = 1)
    {
        var s = new Siparis
        {
            SiparisId = siparisId,
            MusteriId = musteriId,
            SiparisTarihi = DateTime.UtcNow,
            Durum = SiparisDurum.Beklemede,
            ToplamTutar = 0m
        };
        db.Siparisler.Add(s);
        await db.SaveChangesAsync();
        return s;
    }

    public static async Task<SiparisKalemi> SeedSepetKalemiAsync(
        AppDbContext db,
        int siparisId,
        int urunId,
        int adet,
        decimal birimFiyat)
    {
        var kalem = new SiparisKalemi
        {
            SiparisId = siparisId,
            UrunId = urunId,
            Adet = adet,
            BirimFiyat = birimFiyat,
            SatirToplam = adet * birimFiyat
        };

        db.SiparisKalemleri.Add(kalem);
        await db.SaveChangesAsync();
        return kalem;
    }
    public static async Task<Urun> SeedUrunEntityAsync(
        AppDbContext db,
        int urunId = 1,
        int stok = 100,
        bool aktifMi = true,
        decimal fiyat = 10m)
    {
        var urun = new Urun
        {
            UrunId = urunId,
            Ad = "Test Ürün",
            KategoriId = 1,
            Stok = stok,
            AktifMi = aktifMi,
            Fiyat = fiyat
        };

        db.Urunler.Add(urun);
        await db.SaveChangesAsync();
        return urun;
    }

    public static async Task<int> SeedUrunAsync(AppDbContext db, int urunId, string ad, decimal fiyat, int stok, bool aktifMi)
    {
        db.Urunler.Add(new Urun
        {
            UrunId = urunId,
            Ad = ad,
            KategoriId = 1,
            Fiyat = fiyat,
            Stok = stok,
            AktifMi = aktifMi
        });

        await db.SaveChangesAsync();
        return urunId;
    }


    public static async Task<int> SeedMusteriAsync(AppDbContext db, string ad, string soyad, string email)
    {
        db.Musteriler.Add(new MiniMarketCRM.Domain.Entities.Musteri
        {
            Ad = ad,
            Soyad = soyad,
            Email = email,
            OlusturulmaTarihi = DateTime.UtcNow
        });

        await db.SaveChangesAsync();
        return await db.Musteriler.OrderByDescending(x => x.MusteriId).Select(x => x.MusteriId).FirstAsync();
    }
    public static async Task<int> SeedSiparisAsync(
       AppDbContext db,
       int musteriId,
       SiparisDurum durum = SiparisDurum.Beklemede,
       int siparisId = 1,
       DateTime? tarih = null)
    {
        var siparis = new Siparis
        {
            SiparisId = siparisId,
            MusteriId = musteriId,
            SiparisTarihi = tarih ?? DateTime.UtcNow,
            Durum = durum,
            ToplamTutar = 0m
        };

        db.Siparisler.Add(siparis);
        await db.SaveChangesAsync();
        return siparis.SiparisId;
    }
    public static async Task<int> SeedSiparisKalemiAsync(
        AppDbContext db,
        int siparisId,
        int urunId,
        int adet,
        decimal birimFiyat,
        int? kalemId = null)
    {
        var kalem = new SiparisKalemi
        {
            SiparisKalemiId = kalemId ?? 0,
            SiparisId = siparisId,
            UrunId = urunId,
            Adet = adet,
            BirimFiyat = birimFiyat,
            SatirToplam = adet * birimFiyat
        };

        db.SiparisKalemleri.Add(kalem);
        await db.SaveChangesAsync();
        return kalem.SiparisKalemiId;
    }
}

