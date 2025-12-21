using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MiniMarketCRM.Domain.Entities;

namespace MiniMarketCRM.DataAccess.Context
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options){ }
        public DbSet<Kategori> Kategoriler { get; set; }
        public DbSet<Urun> Urunler { get; set; }
        public DbSet<Siparis> Siparisler { get; set; }
        public DbSet<SiparisKalemi> SiparisKalemleri { get; set; }
        public DbSet<Musteri> Musteriler { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            //İlişkiler :
           
            // Müşteri (1) - Sipariş (N) 
            modelBuilder.Entity<Siparis>()
                .HasOne(s =>s.Musteri)
                .WithMany(m =>m.Siparisler)
                .HasForeignKey(s =>s.MusteriId)
                .OnDelete(DeleteBehavior.Restrict); // bu kısımda eğer müşterilerden birisi olurda silinirse ona ait eski siparişler silinmesin diye önlem aldım.

            // Kategori (1) - Ürün (N)
            modelBuilder.Entity<Urun>()
                .HasOne(u => u.Kategori)
                .WithMany(k => k.Urunler)
                .HasForeignKey(u => u.KategoriId)
                .OnDelete(DeleteBehavior.Restrict); // bu kısımda eğer kategorilerden birisi olurda silinirse ona ait eski ürünler silinmesin diye önlem aldım.

            // Sipariş (1) - Sipariş Kalemi (N)
            modelBuilder.Entity<SiparisKalemi>()
                .HasOne(sk => sk.Siparis)
                .WithMany(s => s.SiparisKalemleri)
                .HasForeignKey(sk => sk.SiparisId)
                .OnDelete(DeleteBehavior.Cascade); // bu kısımda eğer sipariş silinirse ona ait sipariş kalemleri de silinsin diye ayarladım.
            
            // Ürün (1) - Sipariş Kalemi (N)
            modelBuilder.Entity<SiparisKalemi>()
                .HasOne(sk => sk.Urun)
                .WithMany(u => u.SiparisKalemleri)
                .HasForeignKey(sk => sk.UrunId)
                .OnDelete(DeleteBehavior.Restrict);

            // Decimal Precision (MSSQL için önemli) 

            modelBuilder.Entity<Urun>()
                .Property(u => u.Fiyat)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Siparis>()
                .Property(s => s.ToplamTutar)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<SiparisKalemi>()
                .Property(sk => sk.BirimFiyat)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<SiparisKalemi>()
                .Property(sk => sk.SatirToplam)
                .HasColumnType("decimal(18,2)");

            // Mail unique 
            modelBuilder.Entity<Musteri>()
                .HasIndex(m => m.Email)
                .IsUnique();

            // aynı ürün tek satır mantığı için (düzeltme alabilir)
            modelBuilder.Entity<SiparisKalemi>()
                .HasIndex(sk => new { sk.SiparisId, sk.UrunId })
                .IsUnique();

        }
    }
}
