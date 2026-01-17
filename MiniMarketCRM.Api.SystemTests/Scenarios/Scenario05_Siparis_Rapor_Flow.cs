using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using MiniMarketCRM.Api.SystemTests.Infrastructure;
using MiniMarketCRM.Application.DTO;
using Xunit;
using static MiniMarketCRM.Api.SystemTests.Infrastructure.HttpTestHelper;

namespace MiniMarketCRM.Api.SystemTests.Scenarios;

/*Scenario 05 — Rapor endpoint’i: tarih aralığında sipariş raporu dönüyor mu? */
public class Scenario05_Siparis_Rapor_Flow
{
    [Fact]
    public async Task E2E_Report_should_return_orders_in_date_range()
    {
        using var factory = new SystemWebApplicationFactory();
        var client = factory.CreateClient();

        // 1) Musteri oluştur
        var musteriEmail = $"rapor_{Guid.NewGuid():N}@test.com";
        var musteriRes = await client.PostAsJsonAsync("/api/musteriler", new MusteriUpsertDTO
        {
            Ad = "Rapor",
            Soyad = "Test",
            Email = musteriEmail
        });

        AssertOkOrCreated(musteriRes, "Musteri.Create");

        
        var musteriId = await ReadIdFromResponse(musteriRes, primaryKey: "musteriId", fallbackKey: "id");
        if (musteriId <= 0)
        {
            var musteriDto = await musteriRes.Content.ReadFromJsonAsync<MusteriDTO>();
            musteriId = musteriDto?.MusteriId ?? 0;
        }
        Assert.True(musteriId > 0, $"MusteriId bulunamadı! Body: {await musteriRes.Content.ReadAsStringAsync()}");

        // 2) Urun oluştur
        var urunAd = $"Rapor Ürün {Guid.NewGuid():N}";
        var urunRes = await client.PostAsJsonAsync("/api/urunler", new UrunUpsertDTO
        {
            Ad = urunAd,
            KategoriId = 1,     // seed var
            Fiyat = 12m,
            Stok = 20,
            AktifMi = true
        });

        AssertOkOrCreated(urunRes, "Urun.Create");

        
        var urunId = await ReadIdFromResponse(urunRes, primaryKey: "urunId", fallbackKey: "id");

        
        if (urunId <= 0)
        {
            var list = await client.GetFromJsonAsync<List<UrunDTO>>("/api/urunler");
            var found = list?.FirstOrDefault(x => x.Ad == urunAd);
            urunId = found?.UrunId ?? 0;
        }

        Assert.True(urunId > 0,
            $"UrunId bulunamadı! Body: {await urunRes.Content.ReadAsStringAsync()}");

        // 3) Sepete ekle
        var addRes = await client.PostAsJsonAsync($"/api/cart/{musteriId}/items",
            new CartItemAddDTO { UrunId = urunId, Adet = 2 });

        
        AssertOkOrCreated(addRes, "Cart.AddItem");

        // 4) Checkout
        var checkoutRes = await client.PostAsync($"/api/cart/{musteriId}/checkout", null);
        Assert.True(
            checkoutRes.StatusCode is HttpStatusCode.OK or HttpStatusCode.Created,
            $"Cart.Checkout beklenmeyen status: {(int)checkoutRes.StatusCode} {checkoutRes.StatusCode}\nBody: {await checkoutRes.Content.ReadAsStringAsync()}"
        );

        
        var from = Uri.EscapeDataString(DateTime.UtcNow.AddDays(-1).ToString("O"));
        var to = Uri.EscapeDataString(DateTime.UtcNow.AddDays(1).ToString("O"));

        var raporRes = await client.GetAsync($"/api/siparisler/rapor?from={from}&to={to}");
        AssertSuccess(raporRes, "Siparis.Rapor");

        var rapor = await ReadJsonOrThrow<List<SiparisRaporDTO>>(raporRes, "Siparis.Rapor.Read");

        Assert.NotNull(rapor);
        Assert.True(rapor.Count > 0, "Rapor boş döndü, en az 1 sipariş bekleniyordu.");

       
        Assert.Contains(rapor, x => x.MusteriId == musteriId);
    }

 
    private static async Task<int> ReadIdFromResponse(HttpResponseMessage res, string primaryKey, string fallbackKey)
    {
        var text = await res.Content.ReadAsStringAsync();
        if (string.IsNullOrWhiteSpace(text)) return 0;

        try
        {
            using var doc = JsonDocument.Parse(text);
            var root = doc.RootElement;

            // urunId / musteriId
            if (TryGetInt(root, primaryKey, out var v1)) return v1;

            // id
            if (TryGetInt(root, fallbackKey, out var v2)) return v2;

           
            if (root.ValueKind == JsonValueKind.Object && root.TryGetProperty("data", out var data))
            {
                if (TryGetInt(data, primaryKey, out var v3)) return v3;
                if (TryGetInt(data, fallbackKey, out var v4)) return v4;
            }
        }
        catch
        {
            // JSON değilse 0 dön
        }

        return 0;
    }

    private static bool TryGetInt(JsonElement obj, string key, out int value)
    {
        value = 0;

       
        if (obj.ValueKind != JsonValueKind.Object) return false;

        foreach (var prop in obj.EnumerateObject())
        {
            if (!string.Equals(prop.Name, key, StringComparison.OrdinalIgnoreCase))
                continue;

            if (prop.Value.ValueKind == JsonValueKind.Number && prop.Value.TryGetInt32(out var i))
            {
                value = i;
                return true;
            }
        }

        return false;
    }
}
