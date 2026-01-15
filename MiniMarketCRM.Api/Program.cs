using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MiniMarketCRM.DataAccess.Context;
using MiniMarketCRM.Application.Interfaces;
using MiniMarketCRM.Application.Services;

namespace MiniMarketCRM.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddScoped<IKategoriService, KategoriService>();
            builder.Services.AddScoped<IUrunService, UrunService>();
            builder.Services.AddScoped<IMusteriService, MusteriService>();
            builder.Services.AddScoped<ISiparisService, SiparisService>();
            builder.Services.AddScoped<ISiparisKalemiService, SiparisKalemiService>();
            builder.Services.AddScoped<ICartService, CartService>();
            builder.Services.AddScoped<ISiparisRaporService, SiparisRaporService>();



            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseAuthorization();
            app.MapControllers();
            app.Run();
        }
    }
}
