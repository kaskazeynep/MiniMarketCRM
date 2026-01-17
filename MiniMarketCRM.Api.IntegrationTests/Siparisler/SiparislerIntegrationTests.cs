using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MiniMarketCRM.Api.IntegrationTests.Infrastructure;
using MiniMarketCRM.Application.DTO;
using MiniMarketCRM.DataAccess.Context;
using MiniMarketCRM.Domain.Enums;
using Xunit;

namespace MiniMarketCRM.Api.IntegrationTests.Siparisler;

public class SiparislerIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public SiparislerIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetAll_should_include_checkout_order()
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

        // Sepete ekle
        var addRes = await client.PostAsJsonAsync($"api/cart/{musteriId}/items",
            new CartItemAddDTO { UrunId = urunId, Adet = 1 });
        Assert.Equal(HttpStatusCode.OK, addRes.StatusCode);

        // Checkout
        var checkoutRes = await client.PostAsync($"api/cart/{musteriId}/checkout", null);
        Assert.Equal(HttpStatusCode.OK, checkoutRes.StatusCode);

        // Act
        var siparisler = await client.GetFromJsonAsync<List<SiparisDTO>>("api/siparisler");

        // Assert
        Assert.NotNull(siparisler);
        Assert.True(siparisler!.Any(x => x.MusteriId == musteriId && x.Durum == SiparisDurum.Tamamlandi));
    }

    [Fact]
    public async Task GetById_should_return_404_when_order_not_found()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var res = await client.GetAsync("api/siparisler/999999");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, res.StatusCode);
    }

    [Fact]
    public async Task PatchDurum_should_update_order_status()
    {
        // Arrange
        var client = _factory.CreateClient();

        int musteriId;
        int urunId;
        int siparisId;

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            musteriId = await TestSeedHelper.GetAnyMusteriIdAsync(db);
            urunId = await TestSeedHelper.GetAnyAktifUrunIdAsync(db);
        }

        // Sepete ekle + checkout => sipariş oluşsun
        var addRes = await client.PostAsJsonAsync($"api/cart/{musteriId}/items",
            new CartItemAddDTO { UrunId = urunId, Adet = 1 });
        Assert.Equal(HttpStatusCode.OK, addRes.StatusCode);

        var checkoutRes = await client.PostAsync($"api/cart/{musteriId}/checkout", null);
        Assert.Equal(HttpStatusCode.OK, checkoutRes.StatusCode);

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            siparisId = await db.Siparisler
                .Where(s => s.MusteriId == musteriId)
                .OrderByDescending(s => s.SiparisId)
                .Select(s => s.SiparisId)
                .FirstAsync();
        }

        // Act: PATCH durum
        var patchRes = await client.PatchAsJsonAsync($"api/siparisler/{siparisId}/durum",
            new SiparisDurumPatchDTO { Durum = SiparisDurum.Iptal });

        // Assert HTTP
        Assert.Equal(HttpStatusCode.OK, patchRes.StatusCode);

        // Assert DB
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var durum = await db.Siparisler.Where(s => s.SiparisId == siparisId).Select(s => s.Durum).FirstAsync();
            Assert.Equal(SiparisDurum.Iptal, durum);
        }
    }

    [Fact]
    public async Task PatchDurum_should_return_400_for_invalid_enum_value()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act: geçersiz enum (999)
        var res = await client.PatchAsJsonAsync("api/siparisler/1/durum",
            new { Durum = 999 });

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);
    }
}
/* TOPLAM 4 ADET TEST YAPILMIŞTIR:
   1) GetAll_should_include_checkout_order
   Cart üzerinden sepete ürün ekleyip checkout yapınca sipariş oluşuyor mu?
   Sonra GET /api/siparisler çağrısında bu sipariş listede görünüyor mu?
   Özellikle Durum == Tamamlandi ve MusteriId == musteriId şartıyla kontrol ediyor
   
   2)GetById_should_return_404_when_order_not_found
   Var olmayan bir sipariş id’si ile GET /api/siparisler/{id} çağrısında API 404 NotFound dönüyor mu?

   3) PatchDurum_should_update_order_status
   Cart ile sipariş oluşturuluyor (add + checkout)
   Sonra PATCH /api/siparisler/{siparisId}/durum ile durum Iptal yapılıyor mu?
   Hem HTTP 200 OK hem de DB’de gerçekten Durum = Iptal oldu mu kontrol ediliyor.

   4) PatchDurum_should_return_400_for_invalid_enum_value
   PATCH /api/siparisler/1/durum çağrısında geçersiz enum değeri (Durum = 999) gönderilince API 400 BadRequest dönüyor mu?
 */ 