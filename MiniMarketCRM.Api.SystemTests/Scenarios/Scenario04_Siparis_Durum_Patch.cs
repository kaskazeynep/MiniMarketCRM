using System.Net;
using System.Net.Http.Json;
using MiniMarketCRM.Api.SystemTests.Infrastructure;
using MiniMarketCRM.Application.DTO;
using MiniMarketCRM.Domain.Enums;
using Xunit;

namespace MiniMarketCRM.Api.SystemTests.Scenarios;

/* Scenario04 : Checkout olmuş siparişin durumunu PATCH ile değiştir*/

public class Scenario04_Siparis_Durum_Patch
{
    [Fact]
    public async Task E2E_Patch_order_status_should_work()
    {
        using var factory = new SystemWebApplicationFactory();
        var client = factory.CreateClient();

        // Musteri
        var musteriRes = await client.PostAsJsonAsync("/api/musteriler", new MusteriUpsertDTO
        {
            Ad = "Patch",
            Soyad = "Test",
            Email = "patch@test.com"
        });
        var musteri = await musteriRes.Content.ReadFromJsonAsync<MusteriDTO>();
        var musteriId = musteri!.MusteriId;

        // Urun
        var urunRes = await client.PostAsJsonAsync("/api/urunler", new UrunUpsertDTO
        {
            Ad = "Patch Ürün",
            KategoriId = 1,
            Fiyat = 8m,
            Stok = 100,
            AktifMi = true
        });
        var urun = await urunRes.Content.ReadFromJsonAsync<UrunDTO>();
        var urunId = urun!.UrunId;

        // Sepete ekle + checkout
        await client.PostAsJsonAsync($"/api/cart/{musteriId}/items",
            new CartItemAddDTO { UrunId = urunId, Adet = 1 });

        await client.PostAsync($"/api/cart/{musteriId}/checkout", null);

        // Siparisleri çek, en son siparişi bul
        var orders = await client.GetFromJsonAsync<List<SiparisDTO>>("/api/siparisler");
        var last = orders!.First(x => x.MusteriId == musteriId);

        // Patch => Iptal
        var patchRes = await client.PatchAsJsonAsync($"/api/siparisler/{last.SiparisId}/durum",
            new SiparisDurumPatchDTO { Durum = SiparisDurum.Iptal });

        Assert.Equal(HttpStatusCode.OK, patchRes.StatusCode);

        // tekrar getir ve kontrol et
        var after = await client.GetFromJsonAsync<SiparisDTO>($"/api/siparisler/{last.SiparisId}");
        Assert.Equal(SiparisDurum.Iptal, after!.Durum);
    }
}
