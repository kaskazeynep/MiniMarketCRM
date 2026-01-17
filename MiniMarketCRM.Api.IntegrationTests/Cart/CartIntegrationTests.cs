using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MiniMarketCRM.Api.IntegrationTests.Infrastructure;
using MiniMarketCRM.Application.DTO;
using MiniMarketCRM.DataAccess.Context;
using MiniMarketCRM.Domain.Enums;
using Xunit;

namespace MiniMarketCRM.Api.IntegrationTests.Cart;

public class CartIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public CartIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task AddItem_should_decrease_product_stock()
    {
        // Arrange
        var client = _factory.CreateClient();

        int musteriId;
        int urunId;
        int stokOnce;

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            musteriId = await TestSeedHelper.GetAnyMusteriIdAsync(db);
            urunId = await TestSeedHelper.GetAnyAktifUrunIdAsync(db);

            stokOnce = await db.Urunler
                .Where(u => u.UrunId == urunId)
                .Select(u => u.Stok)
                .FirstAsync();
        }

        var adet = 3;

        // Act
        var res = await client.PostAsJsonAsync($"api/cart/{musteriId}/items",
            new CartItemAddDTO { UrunId = urunId, Adet = adet });

        // Assert - HTTP
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);

        // Assert - Stock decreased
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var stokSonra = await db.Urunler
                .Where(u => u.UrunId == urunId)
                .Select(u => u.Stok)
                .FirstAsync();

            Assert.Equal(stokOnce - adet, stokSonra);
        }
    }

    [Fact]
    public async Task Cancel_should_restore_product_stock()
    {
        // Arrange
        var client = _factory.CreateClient();

        int musteriId;
        int urunId;
        int stokOnce;

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            musteriId = await TestSeedHelper.GetAnyMusteriIdAsync(db);
            urunId = await TestSeedHelper.GetAnyAktifUrunIdAsync(db);

            stokOnce = await db.Urunler
                .Where(u => u.UrunId == urunId)
                .Select(u => u.Stok)
                .FirstAsync();
        }

        var adet = 4;

        // 1) Sepete ekle -> stok düşsün
        var addRes = await client.PostAsJsonAsync($"api/cart/{musteriId}/items",
            new CartItemAddDTO { UrunId = urunId, Adet = adet });

        Assert.Equal(HttpStatusCode.OK, addRes.StatusCode);

        // 2) Cancel -> stok iade olsun
        var cancelRes = await client.PostAsync($"api/cart/{musteriId}/cancel", content: null);

        // Assert - HTTP
        Assert.Equal(HttpStatusCode.OK, cancelRes.StatusCode);

        // Assert - Stock restored (başlangıç stokuna geri)
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var stokSonra = await db.Urunler
                .Where(u => u.UrunId == urunId)
                .Select(u => u.Stok)
                .FirstAsync();

            Assert.Equal(stokOnce, stokSonra);
        }

    }
    [Fact]
    public async Task Checkout_should_set_order_status_to_Tamamlandi()
    {
        // Arrange
        var client = _factory.CreateClient();

        int musteriId;
        int urunId;

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            musteriId = await TestSeedHelper.GetAnyMusteriIdAsync(db);
            urunId = await TestSeedHelper.GetAnyAktifUrunIdAsync(db);
        }

        // Sepete ürün ekle
        var addRes = await client.PostAsJsonAsync($"api/cart/{musteriId}/items",
            new CartItemAddDTO { UrunId = urunId, Adet = 2 });

        Assert.Equal(HttpStatusCode.OK, addRes.StatusCode);

        // Act
        var checkoutRes = await client.PostAsync($"api/cart/{musteriId}/checkout", content: null);

        // Assert - HTTP
        Assert.Equal(HttpStatusCode.OK, checkoutRes.StatusCode);

        // Assert - DB: Beklemede sipariş kalmamalı, son sipariş Tamamlandi olmalı
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var lastOrder = await db.Siparisler
                .Where(s => s.MusteriId == musteriId)
                .OrderByDescending(s => s.SiparisId)
                .FirstAsync();

            Assert.Equal(MiniMarketCRM.Domain.Enums.SiparisDurum.Tamamlandi, lastOrder.Durum);
        }
    }

    [Fact]
    public async Task AddItem_should_return_400_when_product_is_passive()
    {
        // Arrange
        var client = _factory.CreateClient();

        int musteriId;
        int pasifUrunId;

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            musteriId = await TestSeedHelper.GetAnyMusteriIdAsync(db);

            pasifUrunId = await db.Urunler
                .Where(u => !u.AktifMi)
                .Select(u => u.UrunId)
                .FirstAsync();
        }

        // Act
        var res = await client.PostAsJsonAsync($"api/cart/{musteriId}/items",
            new CartItemAddDTO { UrunId = pasifUrunId, Adet = 1 });

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);
    }

    [Fact]
    public async Task AddItem_should_return_400_when_stock_is_insufficient()
    {
        // Arrange
        var client = _factory.CreateClient();

        int musteriId;
        int urunId;
        int stok;

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            musteriId = await TestSeedHelper.GetAnyMusteriIdAsync(db);
            urunId = await TestSeedHelper.GetAnyAktifUrunIdAsync(db);

            stok = await db.Urunler
                .Where(u => u.UrunId == urunId)
                .Select(u => u.Stok)
                .FirstAsync();
        }

        // Stoktan fazla isteyelim
        var res = await client.PostAsJsonAsync($"api/cart/{musteriId}/items",
            new CartItemAddDTO { UrunId = urunId, Adet = stok + 999 });

        Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);
    }

    [Fact]
    public async Task UpdateItem_increase_should_decrease_stock_and_decrease_should_restore_stock()
    {
        // Arrange
        var client = _factory.CreateClient();

        int musteriId;
        int urunId;
        int stokOnce;

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            musteriId = await TestSeedHelper.GetAnyMusteriIdAsync(db);
            urunId = await TestSeedHelper.GetAnyAktifUrunIdAsync(db);

            stokOnce = await db.Urunler
                .Where(u => u.UrunId == urunId)
                .Select(u => u.Stok)
                .FirstAsync();
        }

        // 1) Sepete 2 adet ekle -> stok 2 düşer
        var addRes = await client.PostAsJsonAsync($"api/cart/{musteriId}/items",
            new CartItemAddDTO { UrunId = urunId, Adet = 2 });
        Assert.Equal(HttpStatusCode.OK, addRes.StatusCode);

        // Sepetten kalemId bul
        int kalemId;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            kalemId = await db.SiparisKalemleri
                .Include(k => k.Siparis)
                .Where(k => k.Siparis.MusteriId == musteriId && k.Siparis.Durum == MiniMarketCRM.Domain.Enums.SiparisDurum.Beklemede)
                .Where(k => k.UrunId == urunId)
                .Select(k => k.SiparisKalemiId)
                .FirstAsync();
        }

        // 2) Update artır: 2 -> 5 (fark +3) => stok +3 düşer
        var incRes = await client.PutAsJsonAsync($"api/cart/{musteriId}/items/{kalemId}",
            new CartItemUpdateDTO { Adet = 5 });
        Assert.Equal(HttpStatusCode.OK, incRes.StatusCode);

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var stokAfterIncrease = await db.Urunler.Where(u => u.UrunId == urunId).Select(u => u.Stok).FirstAsync();

            // başlangıç - 5 olmalı (çünkü sepette artık 5 adet var)
            Assert.Equal(stokOnce - 5, stokAfterIncrease);
        }

        // 3) Update azalt: 5 -> 1 (fark -4) => stok +4 iade
        var decRes = await client.PutAsJsonAsync($"api/cart/{musteriId}/items/{kalemId}",
            new CartItemUpdateDTO { Adet = 1 });
        Assert.Equal(HttpStatusCode.OK, decRes.StatusCode);

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var stokAfterDecrease = await db.Urunler.Where(u => u.UrunId == urunId).Select(u => u.Stok).FirstAsync();

            // başlangıç - 1 olmalı (çünkü sepette artık 1 adet var)
            Assert.Equal(stokOnce - 1, stokAfterDecrease);
        }
    }
    [Fact]
    public async Task GetCart_should_return_empty_cart_when_no_active_cart_exists()
    {
        // Arrange
        var client = _factory.CreateClient();
        int musteriId;

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            musteriId = await TestSeedHelper.GetAnyMusteriIdAsync(db);

            // Bu müşteri için aktif (Beklemede) sepet varsa iptal edelim ki test deterministik olsun
            var active = await db.Siparisler
                .Where(s => s.MusteriId == musteriId && s.Durum == SiparisDurum.Beklemede)
                .ToListAsync();

            if (active.Any())
            {
                db.Siparisler.RemoveRange(active);
                await db.SaveChangesAsync();
            }
        }

        // Act
        var cart = await client.GetFromJsonAsync<CartDTO>($"api/cart/{musteriId}");

        // Assert
        Assert.NotNull(cart);
        Assert.Equal(0, cart!.SiparisId);
        Assert.Equal(musteriId, cart.MusteriId);
        Assert.NotNull(cart.Items);
        Assert.Empty(cart.Items);
        Assert.Equal(0m, cart.ToplamTutar);
    }

  
}

/* toplam 7 adet test 
    1)AddItem_should_decrease_product_stock
    Sepete ürün ekleyince stok düşüyor mu? (Evet: stokOnce - adet kontrol ediliyor)

    2)Cancel_should_restore_product_stock
    Sepete ekledikten sonra cancel yapınca stok geri iade oluyor mu? (Evet: stok başlangıca dönüyor)

    3)Checkout_should_set_order_status_to_Tamamlandi
    Sepete ürün ekleyip checkout yapınca sipariş durumu Tamamlandi oluyor mu? (DB’den son sipariş çekilip Durum assert ediliyor)

    4)AddItem_should_return_400_when_product_is_passive
    Pasif ürün sepete eklenemesin → API 400 BadRequest dönüyor mu?

    5)AddItem_should_return_400_when_stock_is_insufficient
    Stok yetersizken sepete eklenemesin → API 400 BadRequest dönüyor mu?

    6)UpdateItem_increase_should_decrease_stock_and_decrease_should_restore_stock
    Sepetteki kalemi artırınca stok daha da düşüyor mu?
    Kalemi azaltınca stok iade oluyor mu?
    (2→5 artışında toplam 5’e göre stok, 5→1 düşüşünde toplam 1’e göre stok kontrol ediliyor)

    7)GetCart_should_return_empty_cart_when_no_active_cart_exists
    Aktif sepet yoksa boş sepet dönüyor mu? (SiparisId=0, Items boş, ToplamTutar=0m)
 
 */