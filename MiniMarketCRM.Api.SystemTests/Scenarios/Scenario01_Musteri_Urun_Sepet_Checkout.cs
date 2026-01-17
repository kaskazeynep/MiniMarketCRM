using System.Net;
using System.Net.Http.Json;
using MiniMarketCRM.Api.SystemTests.Infrastructure;
using MiniMarketCRM.Application.DTO;
using MiniMarketCRM.Domain.Enums;
using Xunit;
using static MiniMarketCRM.Api.SystemTests.Infrastructure.HttpTestHelper;

namespace MiniMarketCRM.Api.SystemTests.Scenarios;

/* Scenario 01 — Kullanıcı (Müşteri) → Ürün → Sepete Ekle → Checkout → Sipariş oluştu*/
public class Scenario01_Musteri_Urun_Sepet_Checkout
{
    [Fact]
    public async Task E2E_Musteri_Urun_Sepet_Checkout_flow_should_work()
    {
        using var factory = new SystemWebApplicationFactory();
        var client = factory.CreateClient();

        // 1) Musteri oluştur (email unique)
        var email = $"zeynep.e2e1_{Guid.NewGuid():N}@test.com";

        var musteriCreateRes = await client.PostAsJsonAsync("/api/musteriler", new MusteriUpsertDTO
        {
            Ad = "Zeynep",
            Soyad = "Kaska",
            Email = email
        });

        AssertOkOrCreated(musteriCreateRes, "MusteriCreate");

        var musteriDto = await TryReadDto<MusteriDTO>(musteriCreateRes);
        int musteriId;

        if (musteriDto is not null && musteriDto.MusteriId > 0)
        {
            musteriId = musteriDto.MusteriId;
        }
        else
        {
            // Fallback: listeden email ile bul
            var musteriListRes = await client.GetAsync("/api/musteriler");
            AssertSuccess(musteriListRes, "Musteriler.GetAll");

            var musteriList = await ReadJsonOrThrow<List<MusteriDTO>>(musteriListRes, "Musteriler.Read");
            musteriId = musteriList.First(x => x.Email.Equals(email, StringComparison.OrdinalIgnoreCase)).MusteriId;
        }

        // 2) Urun oluştur 
        var urunAd = $"E2E Test Ürün {Guid.NewGuid():N}";

        var urunCreateRes = await client.PostAsJsonAsync("/api/urunler", new UrunUpsertDTO
        {
            Ad = urunAd,
            KategoriId = 1,   // factory seed ediyor
            Fiyat = 10m,
            Stok = 50,
            AktifMi = true
        });

        AssertOkOrCreated(urunCreateRes, "UrunCreate");

        var urunDto = await TryReadDto<UrunDTO>(urunCreateRes);
        int urunId;

        if (urunDto is not null && urunDto.UrunId > 0)
        {
            urunId = urunDto.UrunId;
        }
        else
        {
            // Fallback: listeden ad ile bul
            var urunListRes = await client.GetAsync("/api/urunler");
            AssertSuccess(urunListRes, "Urunler.GetAll");

            var urunList = await ReadJsonOrThrow<List<UrunDTO>>(urunListRes, "Urunler.Read");
            urunId = urunList.First(x => x.Ad == urunAd).UrunId;
        }

        // ekstra güvenlik (id 0 ise direkt fail edip body bas)
        Assert.True(urunId > 0, $"UrunId 0 geldi. UrunCreate Body:\n{await urunCreateRes.Content.ReadAsStringAsync()}");

        // 3) Sepete ekle
        var addRes = await client.PostAsJsonAsync($"/api/cart/{musteriId}/items",
            new CartItemAddDTO { UrunId = urunId, Adet = 2 });

        AssertSuccess(addRes, "CartAdd");

        // 4) Checkout
        var checkout = await client.PostAsync($"/api/cart/{musteriId}/checkout", null);
        AssertSuccess(checkout, "Checkout");

        // 5) Sipariş listesinde tamamlandi var mı?
        var siparisListRes = await client.GetAsync("/api/siparisler");
        AssertSuccess(siparisListRes, "Siparisler.GetAll");

        var siparisler = await ReadJsonOrThrow<List<SiparisDTO>>(siparisListRes, "Siparisler.Read");

        Assert.Contains(siparisler,
            s => s.MusteriId == musteriId && s.Durum == SiparisDurum.Tamamlandi);
    }
    private static async Task<T?> TryReadDto<T>(HttpResponseMessage res) where T : class
    {
        var text = await res.Content.ReadAsStringAsync();

        try
        {
            return System.Text.Json.JsonSerializer.Deserialize<T>(text, new System.Text.Json.JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        catch
        {
            return null;
        }
    }
}
