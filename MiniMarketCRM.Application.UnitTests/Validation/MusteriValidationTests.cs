using FluentAssertions;
using MiniMarketCRM.Application.DTO;
using MiniMarketCRM.Application.Services;
using MiniMarketCRM.Application.UnitTests.Helpers;
using Xunit;

namespace MiniMarketCRM.Application.UnitTests.Validation;

public class MusteriValidationTests
{
    [Fact]
    public async Task Validate_should_throw_when_ad_is_empty()
    {
        using var db = TestDataFactory.CreateDbContext();
        var sut = new MusteriService(db);

        var dto = new MusteriUpsertDTO
        {
            Ad = "   ",
            Soyad = "Kaska",
            Email = "zeynep@mail.com"
        };

        var act = () => sut.CreateAsync(dto);

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*Ad*");
    }

    [Fact]
    public async Task Validate_should_throw_when_soyad_is_empty()
    {
        using var db = TestDataFactory.CreateDbContext();
        var sut = new MusteriService(db);

        var dto = new MusteriUpsertDTO
        {
            Ad = "Zeynep",
            Soyad = "",
            Email = "zeynep@mail.com"
        };

        var act = () => sut.CreateAsync(dto);

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*Soyad*");
    }

    [Fact]
    public async Task Validate_should_throw_when_email_is_empty()
    {
        using var db = TestDataFactory.CreateDbContext();
        var sut = new MusteriService(db);

        var dto = new MusteriUpsertDTO
        {
            Ad = "Zeynep",
            Soyad = "Kaska",
            Email = "   "
        };

        var act = () => sut.CreateAsync(dto);

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*Email*");
    }
}

/* 3 unit test yazıldı
 * 1) Validate_should_throw_when_ad_is_empty
 * 2) Validate_should_throw_when_soyad_is_empty
 * 3) Validate_should_throw_when_email_is_empty
 */