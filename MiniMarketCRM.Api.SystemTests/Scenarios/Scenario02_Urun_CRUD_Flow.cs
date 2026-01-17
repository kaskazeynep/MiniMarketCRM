using System.Net;
using System.Net.Http.Json;
using MiniMarketCRM.Api.SystemTests.Infrastructure;
using MiniMarketCRM.Application.DTO;
using Xunit;
using static MiniMarketCRM.Api.SystemTests.Infrastructure.HttpTestHelper;

namespace MiniMarketCRM.Api.SystemTests.Scenarios;

/* Scenario02 : Ürün liste → detay → güncelle → sil */
public class Scenario02_Urun_CRUD_Flow
{
    [Fact]
    public async Task E2E_Urun_CRUD_flow_should_work()
    {
        using var factory = new SystemWebApplicationFactory();
        var client = factory.CreateClient();

        // Ürün adı unique olsun ki listeden kesin bulayımm
        var urunAd = $"CRUD Ürün {Guid.NewGuid():N}";

        // 1) CREATE
        var createRes = await client.PostAsJsonAsync("/api/urunler", new UrunUpsertDTO
        {
            Ad = urunAd,
            KategoriId = 1, // seed'li
            Fiyat = 15m,
            Stok = 10,
            AktifMi = true
        });

        AssertOkOrCreated(createRes, "Urun.Create");

        // ID'yi güvenli çek: body'den gelmezse listeden bul
        var createdDto = await TryReadDto<UrunDTO>(createRes);
        int id;

        if (createdDto is not null && createdDto.UrunId > 0)
        {
            id = createdDto.UrunId;
        }
        else
        {
            var listRes0 = await client.GetAsync("/api/urunler");
            AssertSuccess(listRes0, "Urun.List(AfterCreate)");

            var list0 = await ReadJsonOrThrow<List<UrunDTO>>(listRes0, "Urun.List(AfterCreate).Read");
            id = list0.First(x => x.Ad == urunAd).UrunId;
        }

        Assert.True(id > 0, $"UrunId 0 geldi. Create Body:\n{await createRes.Content.ReadAsStringAsync()}");

        // 2) LIST
        var listRes = await client.GetAsync("/api/urunler");
        AssertSuccess(listRes, "Urun.List");

        var list = await ReadJsonOrThrow<List<UrunDTO>>(listRes, "Urun.List.Read");
        Assert.Contains(list, x => x.UrunId == id);

        // 3) DETAIL
        var detailRes = await client.GetAsync($"/api/urunler/{id}");
        AssertSuccess(detailRes, "Urun.Detail");

        var detail = await ReadJsonOrThrow<UrunDTO>(detailRes, "Urun.Detail.Read");
        Assert.Equal(urunAd, detail.Ad);

        // 4) UPDATE (PUT)
        var urunAdGuncel = $"{urunAd} - Güncel";

        var updateRes = await client.PutAsJsonAsync($"/api/urunler/{id}", new UrunUpsertDTO
        {
            Ad = urunAdGuncel,
            KategoriId = 1,
            Fiyat = 20m,
            Stok = 30,
            AktifMi = true
        });

        Assert.True(updateRes.StatusCode is HttpStatusCode.OK or HttpStatusCode.NoContent,
            $"Urun.Update beklenmeyen status: {(int)updateRes.StatusCode} {updateRes.StatusCode}\nBody: {await updateRes.Content.ReadAsStringAsync()}");

        // 5) DETAIL (AFTER UPDATE)
        var afterUpdateRes = await client.GetAsync($"/api/urunler/{id}");
        AssertSuccess(afterUpdateRes, "Urun.DetailAfterUpdate");

        var afterUpdate = await ReadJsonOrThrow<UrunDTO>(afterUpdateRes, "Urun.DetailAfterUpdate.Read");
        Assert.Equal(urunAdGuncel, afterUpdate.Ad);
        Assert.Equal(20m, afterUpdate.Fiyat);
        Assert.Equal(30, afterUpdate.Stok);

        // 6) DELETE
        var deleteRes = await client.DeleteAsync($"/api/urunler/{id}");

        Assert.True(deleteRes.StatusCode is HttpStatusCode.OK or HttpStatusCode.NoContent,
            $"Urun.Delete beklenmeyen status: {(int)deleteRes.StatusCode} {deleteRes.StatusCode}\nBody: {await deleteRes.Content.ReadAsStringAsync()}");

        // 7) SHOULD BE 404 AFTER DELETE
        var afterDeleteRes = await client.GetAsync($"/api/urunler/{id}");
        Assert.Equal(HttpStatusCode.NotFound, afterDeleteRes.StatusCode);
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
