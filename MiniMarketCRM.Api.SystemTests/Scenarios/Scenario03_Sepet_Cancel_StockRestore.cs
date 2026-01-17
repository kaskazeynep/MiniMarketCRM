using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using MiniMarketCRM.Api.SystemTests.Infrastructure;
using MiniMarketCRM.Application.DTO;
using Xunit;
using static MiniMarketCRM.Api.SystemTests.Infrastructure.HttpTestHelper;

namespace MiniMarketCRM.Api.SystemTests.Scenarios;

/*Scenario03 : Sepete ekle → Cancel → stok geri gelsin */
public class Scenario03_Sepet_Cancel_StockRestore
{
    [Fact]
    public async Task E2E_Sepet_cancel_should_restore_stock()
    {
        using var factory = new SystemWebApplicationFactory();
        var client = factory.CreateClient();

        // 1) Musteri oluştur
        var email = $"cancel_{Guid.NewGuid():N}@test.com";
        var musteriRes = await client.PostAsJsonAsync("/api/musteriler", new MusteriUpsertDTO
        {
            Ad = "Cancel",
            Soyad = "Test",
            Email = email
        });

        AssertOkOrCreated(musteriRes, "Musteri.Create");

        var musteriId = await ReadIdFromResponse(musteriRes, primaryKey: "musteriId", fallbackKey: "id");
        if (musteriId <= 0)
        {
            var dto = await musteriRes.Content.ReadFromJsonAsync<MusteriDTO>();
            musteriId = dto?.MusteriId ?? 0;
        }
        Assert.True(musteriId > 0, $"MusteriId bulunamadı! Body: {await musteriRes.Content.ReadAsStringAsync()}");

        // 2) Urun oluştur
        var urunAd = $"Stok Ürün {Guid.NewGuid():N}";
        var urunRes = await client.PostAsJsonAsync("/api/urunler", new UrunUpsertDTO
        {
            Ad = urunAd,
            KategoriId = 1,   // seed var
            Fiyat = 5m,
            Stok = 10,
            AktifMi = true
        });

        AssertOkOrCreated(urunRes, "Urun.Create");

        var urunId = await ReadIdFromResponse(urunRes, primaryKey: "urunId", fallbackKey: "id");

        // Eğer create response body DTO dönmüyorsa listeden bul
        if (urunId <= 0)
        {
            var list = await client.GetFromJsonAsync<List<UrunDTO>>("/api/urunler");
            var found = list?.FirstOrDefault(x => x.Ad == urunAd);
            urunId = found?.UrunId ?? 0;
        }
        Assert.True(urunId > 0, $"UrunId bulunamadı! Body: {await urunRes.Content.ReadAsStringAsync()}");

        // 3) Stok önce (OK garanti)
        var beforeRes = await client.GetAsync($"/api/urunler/{urunId}");
        AssertSuccess(beforeRes, "Urun.Get(before)");
        var before = await ReadJsonOrThrow<UrunDTO>(beforeRes, "Urun.Get(before).Read");
        var stokOnce = before.Stok;

        // 4) Sepete ekle (3)
        var addRes = await client.PostAsJsonAsync($"/api/cart/{musteriId}/items",
            new CartItemAddDTO { UrunId = urunId, Adet = 3 });

        AssertOkOrCreated(addRes, "Cart.AddItem");

        // 5) Cancel
        var cancelRes = await client.PostAsync($"/api/cart/{musteriId}/cancel", null);

       
        Assert.True(
            cancelRes.StatusCode is HttpStatusCode.OK or HttpStatusCode.NoContent,
            $"Cart.Cancel beklenmeyen status: {(int)cancelRes.StatusCode} {cancelRes.StatusCode}\nBody: {await cancelRes.Content.ReadAsStringAsync()}"
        );

        // 6) Stok sonra
        var afterRes = await client.GetAsync($"/api/urunler/{urunId}");
        AssertSuccess(afterRes, "Urun.Get(after)");
        var after = await ReadJsonOrThrow<UrunDTO>(afterRes, "Urun.Get(after).Read");

        // 7) Assert: stok başa dönmeli
        Assert.Equal(stokOnce, after.Stok);
    }

    private static async Task<int> ReadIdFromResponse(HttpResponseMessage res, string primaryKey, string fallbackKey)
    {
        var text = await res.Content.ReadAsStringAsync();
        if (string.IsNullOrWhiteSpace(text)) return 0;

        try
        {
            using var doc = JsonDocument.Parse(text);
            var root = doc.RootElement;

            if (TryGetInt(root, primaryKey, out var v1)) return v1;
            if (TryGetInt(root, fallbackKey, out var v2)) return v2;

            if (root.ValueKind == JsonValueKind.Object && root.TryGetProperty("data", out var data))
            {
                if (TryGetInt(data, primaryKey, out var v3)) return v3;
                if (TryGetInt(data, fallbackKey, out var v4)) return v4;
            }
        }
        catch
        {
            // JSON değilse 0
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
