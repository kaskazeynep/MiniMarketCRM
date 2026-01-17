using Microsoft.EntityFrameworkCore;
using MiniMarketCRM.DataAccess.Context;
using MiniMarketCRM.Domain.Entities;

namespace MiniMarketCRM.Api.IntegrationTests.Infrastructure;

public static class TestSeedHelper
{
    public static async Task SeedBasicAsync(AppDbContext db)
    {
        // Aynı test host içinde tekrar çağrılırsa duplicate olmasın diye
        if (db.Musteriler.Any() || db.Urunler.Any() || db.Kategoriler.Any())
            return;

        // 1) Kategoriler (Urun.KategoriId zorunlu)
        var manav = new Kategori { KategoriAdi = "Manav" };
        var market = new Kategori { KategoriAdi = "Market" };
        db.Kategoriler.AddRange(manav, market);
        await db.SaveChangesAsync();

        // 2) Müşteriler (Email unique)
        var m1 = new Musteri
        {
            Ad = "Zeynep",
            Soyad = "Kaska",
            Email = "zeynep@test.com",
            OlusturulmaTarihi = DateTime.UtcNow
        };

        var m2 = new Musteri
        {
            Ad = "Ali",
            Soyad = "Veli",
            Email = "ali@test.com",
            OlusturulmaTarihi = DateTime.UtcNow
        };

        db.Musteriler.AddRange(m1, m2);
        await db.SaveChangesAsync();

        // 3) Ürünler (KategoriId + fiyat/stok)
        var u1 = new Urun
        {
            Ad = "Domates",
            KategoriId = manav.KategoriId,
            Fiyat = 25.50m,
            Stok = 100,
            AktifMi = true
        };

        var u2 = new Urun
        {
            Ad = "Süt 1L",
            KategoriId = market.KategoriId,
            Fiyat = 37.90m,
            Stok = 50,
            AktifMi = true
        };

        var u3 = new Urun
        {
            Ad = "Pasif Ürün",
            KategoriId = market.KategoriId,
            Fiyat = 10m,
            Stok = 10,
            AktifMi = false
        };

        db.Urunler.AddRange(u1, u2, u3);
        await db.SaveChangesAsync();
    }

    public static async Task<int> GetAnyMusteriIdAsync(AppDbContext db)
        => await db.Musteriler.Select(x => x.MusteriId).FirstAsync();

    public static async Task<int> GetAnyAktifUrunIdAsync(AppDbContext db)
        => await db.Urunler.Where(x => x.AktifMi).Select(x => x.UrunId).FirstAsync();

    public static async Task<int> GetAnyKategoriIdAsync(AppDbContext db)
        => await db.Kategoriler.Select(x => x.KategoriId).FirstAsync();
}
