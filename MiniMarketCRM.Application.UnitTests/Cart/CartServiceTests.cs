using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MiniMarketCRM.Application.DTO;
using MiniMarketCRM.Application.Interfaces;
using MiniMarketCRM.Application.Services;
using MiniMarketCRM.Application.UnitTests.Helpers;
using MiniMarketCRM.Domain.Enums;
using Moq;
using Xunit;

namespace MiniMarketCRM.Application.UnitTests.Cart;

public class CartServiceTests
{
    [Fact]
    public async Task AddItemAsync_should_throw_when_adet_is_zero_or_negative()
    {
        using var db = TestDataFactory.CreateDbContext();
        await TestDataFactory.SeedMusteriAsync(db);

        var kalemService = new Mock<ISiparisKalemiService>();
        var sut = new CartService(db, kalemService.Object);

        var act0 = () => sut.AddItemAsync(1, new CartItemAddDTO { UrunId = 1, Adet = 0 });
        await act0.Should().ThrowAsync<ArgumentException>().WithMessage("*Adet*");

        var actNeg = () => sut.AddItemAsync(1, new CartItemAddDTO { UrunId = 1, Adet = -1 });
        await actNeg.Should().ThrowAsync<ArgumentException>().WithMessage("*Adet*");
    }

    [Fact]
    public async Task AddItemAsync_should_throw_when_musteri_not_found()
    {
        using var db = TestDataFactory.CreateDbContext();

        var kalemService = new Mock<ISiparisKalemiService>();
        var sut = new CartService(db, kalemService.Object);

        var act = () => sut.AddItemAsync(999, new CartItemAddDTO { UrunId = 1, Adet = 1 });

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*MusteriId*");
    }

    [Fact]
    public async Task GetOrCreateAsync_should_return_empty_cart_when_no_active_cart_exists()
    {
        using var db = TestDataFactory.CreateDbContext();
        var musteriId = await TestDataFactory.SeedMusteriAsync(db);

        var kalemService = new Mock<ISiparisKalemiService>();
        var sut = new CartService(db, kalemService.Object);

        var cart = await sut.GetOrCreateAsync(musteriId);

        cart.SiparisId.Should().Be(0);
        cart.MusteriId.Should().Be(musteriId);
        cart.Items.Should().NotBeNull();
        cart.Items.Should().BeEmpty();
        cart.ToplamTutar.Should().Be(0m);
    }

    [Fact]
    public async Task AddItemAsync_should_create_cart_when_not_exists_and_call_kalemService_add()
    {
        using var db = TestDataFactory.CreateDbContext();
        var musteriId = await TestDataFactory.SeedMusteriAsync(db);

    
        await TestDataFactory.SeedUrunAsync(db, urunId: 5, ad: "Urun-5", fiyat: 10m, stok: 100, aktifMi: true);

        var kalemService = new Mock<ISiparisKalemiService>();

       
        kalemService
            .Setup(x => x.AddAsync(It.IsAny<int>(), It.IsAny<SiparisKalemiUpsertDTO>()))
            .ReturnsAsync((int siparisId, SiparisKalemiUpsertDTO dto) =>
                new SiparisKalemiDTO
                {
                    SiparisKalemiId = 1,
                    SiparisId = siparisId,
                    UrunId = dto.UrunId,
                    Adet = dto.Adet,
                    BirimFiyat = 10m,
                    SatirToplam = dto.Adet * 10m
                });

        var sut = new CartService(db, kalemService.Object);

        await sut.AddItemAsync(musteriId, new CartItemAddDTO { UrunId = 5, Adet = 3 });

        var sepet = await db.Siparisler
            .FirstOrDefaultAsync(s => s.MusteriId == musteriId && s.Durum == SiparisDurum.Beklemede);

        sepet.Should().NotBeNull();

        kalemService.Verify(x => x.AddAsync(
            sepet!.SiparisId,
            It.Is<SiparisKalemiUpsertDTO>(d => d.UrunId == 5 && d.Adet == 3 && d.SiparisId == sepet.SiparisId)
        ), Times.Once);
    }

    [Fact]
    public async Task UpdateItemAsync_should_throw_when_active_cart_not_found()
    {
        using var db = TestDataFactory.CreateDbContext();
        await TestDataFactory.SeedMusteriAsync(db);

        var kalemService = new Mock<ISiparisKalemiService>();
        var sut = new CartService(db, kalemService.Object);

        var act = () => sut.UpdateItemAsync(1, kalemId: 10, new CartItemUpdateDTO { Adet = 2 });

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("*Aktif sepet*");
    }
}


/* 5 unit test 
    1) AddItemAsync_should_throw_when_adet_is_zero_or_negative
    2) AddItemAsync_should_throw_when_musteri_not_found
    3) GetOrCreateAsync_should_return_empty_cart_when_no_active_cart_exists
    4) AddItemAsync_should_create_cart_when_not_exists_and_call_kalemService_add
    5) UpdateItemAsync_should_throw_when_active_cart_not_found
*/