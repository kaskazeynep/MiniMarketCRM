using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MiniMarketCRM.Application.DTO;
using MiniMarketCRM.Application.Services;
using MiniMarketCRM.Application.UnitTests.Helpers;
using MiniMarketCRM.Domain.Enums;
using Xunit;

namespace MiniMarketCRM.Application.UnitTests.Siparisler;

public class SiparisServiceTests
{
    [Fact]
    public async Task CreateAsync_should_throw_when_musteri_not_found()
    {
        using var db = TestDataFactory.CreateDbContext();
        var sut = new SiparisService(db);

        var act = () => sut.CreateAsync(new SiparisUpsertDTO
        {
            MusteriId = 999,
            SiparisTarihi = DateTime.UtcNow,
            Durum = SiparisDurum.Beklemede
        });

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*MusteriId*");
    }

    [Fact]
    public async Task GetByIdAsync_should_return_null_when_not_found()
    {
        using var db = TestDataFactory.CreateDbContext();
        var sut = new SiparisService(db);

        var result = await sut.GetByIdAsync(123456);

        result.Should().BeNull();
    }

    [Fact]
    public async Task UpdateAsync_should_recalculate_total_from_order_items()
    {
        using var db = TestDataFactory.CreateDbContext();

        var musteriId = await TestDataFactory.SeedMusteriAsync(db);
        var siparisId = await TestDataFactory.SeedSiparisAsync(db, musteriId, SiparisDurum.Beklemede, siparisId: 1);

       
        await TestDataFactory.SeedUrunEntityAsync(db, urunId: 1, fiyat: 10m);
        await TestDataFactory.SeedUrunEntityAsync(db, urunId: 2, fiyat: 20m);

        // Kalemler: 2*10=20 ve 1*20=20 => toplam 40
        await TestDataFactory.SeedSiparisKalemiAsync(db, siparisId, urunId: 1, adet: 2, birimFiyat: 10m);
        await TestDataFactory.SeedSiparisKalemiAsync(db, siparisId, urunId: 2, adet: 1, birimFiyat: 20m);

        var sut = new SiparisService(db);

        var updated = await sut.UpdateAsync(siparisId, new SiparisUpsertDTO
        {
            MusteriId = musteriId,
            SiparisTarihi = DateTime.UtcNow,
            Durum = SiparisDurum.Tamamlandi
        });

        updated.Should().NotBeNull();
        updated!.ToplamTutar.Should().Be(40m);
        updated.Durum.Should().Be(SiparisDurum.Tamamlandi);

        var entity = await db.Siparisler.FirstAsync(x => x.SiparisId == siparisId);
        entity.ToplamTutar.Should().Be(40m);
        entity.Durum.Should().Be(SiparisDurum.Tamamlandi);
    }

    [Fact]
    public async Task DeleteAsync_should_return_false_when_order_not_found()
    {
        using var db = TestDataFactory.CreateDbContext();
        var sut = new SiparisService(db);

        var ok = await sut.DeleteAsync(999999);

        ok.Should().BeFalse();
    }
}
/* 4 unit test yazıldı
 * 1) CreateAsync_should_throw_when_musteri_not_found
 * 2) GetByIdAsync_should_return_null_when_not_found
 * 3) UpdateAsync_should_recalculate_total_from_order_items
 * 4) DeleteAsync_should_return_false_when_order_not_found
 */