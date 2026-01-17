using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MiniMarketCRM.Application.DTO;
using MiniMarketCRM.Application.Services;
using MiniMarketCRM.Application.UnitTests.Helpers;
using Xunit;

namespace MiniMarketCRM.Application.UnitTests.Musteri;

public class MusteriServiceTests
{
    [Fact]
    public async Task CreateAsync_should_throw_when_email_already_exists()
    {
        using var db = TestDataFactory.CreateDbContext();
        var sut = new MusteriService(db);

        // Aynı email ile önce seed
        db.Musteriler.Add(new MiniMarketCRM.Domain.Entities.Musteri
        {
            Ad = "A",
            Soyad = "B",
            Email = "test@mail.com",
            OlusturulmaTarihi = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        var dto = new MusteriUpsertDTO
        {
            Ad = "Zeynep",
            Soyad = "Kaska",
            Email = "test@mail.com"
        };

        var act = () => sut.CreateAsync(dto);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*email*");
    }

    [Fact]
    public async Task CreateAsync_should_trim_fields_and_save()
    {
        using var db = TestDataFactory.CreateDbContext();
        var sut = new MusteriService(db);

        var dto = new MusteriUpsertDTO
        {
            Ad = "  Zeynep  ",
            Soyad = "  Kaska ",
            Email = "  zeynep@mail.com  "
        };

        var created = await sut.CreateAsync(dto);

        created.MusteriId.Should().BeGreaterThan(0);
        created.Ad.Should().Be("Zeynep");
        created.Soyad.Should().Be("Kaska");
        created.Email.Should().Be("zeynep@mail.com");

        var entity = await db.Musteriler.FirstAsync(x => x.MusteriId == created.MusteriId);
        entity.Ad.Should().Be("Zeynep");
        entity.Soyad.Should().Be("Kaska");
        entity.Email.Should().Be("zeynep@mail.com");
    }

    [Fact]
    public async Task UpdateAsync_should_throw_when_email_used_by_another_customer()
    {
        using var db = TestDataFactory.CreateDbContext();
        var sut = new MusteriService(db);

        // iki müşteri
        var m1 = new MiniMarketCRM.Domain.Entities.Musteri
        {
            Ad = "Zeynep",
            Soyad = "Kaska",
            Email = "z1@mail.com",
            OlusturulmaTarihi = DateTime.UtcNow
        };
        var m2 = new MiniMarketCRM.Domain.Entities.Musteri
        {
            Ad = "Ayse",
            Soyad = "Yilmaz",
            Email = "z2@mail.com",
            OlusturulmaTarihi = DateTime.UtcNow
        };

        db.Musteriler.AddRange(m1, m2);
        await db.SaveChangesAsync();

        // m1'i update ederken m2'nin email'ini vermeye çalışalım
        var dto = new MusteriUpsertDTO
        {
            Ad = "Zeynep",
            Soyad = "Kaska",
            Email = "z2@mail.com"
        };

        var act = () => sut.UpdateAsync(m1.MusteriId, dto);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*başka bir müşteride*");
    }

    [Fact]
    public async Task DeleteAsync_should_return_false_when_not_found()
    {
        using var db = TestDataFactory.CreateDbContext();
        var sut = new MusteriService(db);

        var result = await sut.DeleteAsync(999999);

        result.Should().BeFalse();
    }
}
/* 4 unit test yazıldı
 * 1) CreateAsync_should_throw_when_email_already_exists
 * 2) CreateAsync_should_trim_fields_and_save
 * 3) UpdateAsync_should_throw_when_email_used_by_another_customer
 * 4) DeleteAsync_should_return_false_when_not_found
 */