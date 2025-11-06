using Microsoft.EntityFrameworkCore;
using Domain.Entities;
using Microsoft.EntityFrameworkCore.Design;

namespace Infrastructure.Data;

public class AppDbContext : DbContext
{
    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Cari> Cariler => Set<Cari>();
    public DbSet<CariAdres> CariAdresler => Set<CariAdres>();
    public DbSet<Stok> Stoklar => Set<Stok>();
    public DbSet<StokFiyat> StokFiyatlar => Set<StokFiyat>();
    public DbSet<Teklif> Teklifler => Set<Teklif>();
    public DbSet<TeklifKalem> TeklifKalemler => Set<TeklifKalem>();
    public DbSet<TeklifSepet> TeklifSepetler => Set<TeklifSepet>();
    public DbSet<TeklifSepetKalem> TeklifSepetKalemler => Set<TeklifSepetKalem>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) {}
    public AppDbContext() {} // Migration ve CLI için

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.Entity<User>().HasIndex(x => x.Email).IsUnique();
        modelBuilder.Entity<User>().HasIndex(x => x.UserName).IsUnique();
        // Phone için unique index (NULL değerler unique constraint'i ihlal etmez)
        modelBuilder.Entity<User>().HasIndex(x => x.Phone).IsUnique().HasFilter("[Phone] IS NOT NULL");
        modelBuilder.Entity<RefreshToken>().HasIndex(x => x.Token).IsUnique();
        modelBuilder.Entity<User>().Property(x => x.Role).HasDefaultValue("user");
        modelBuilder.Entity<User>().Property(x => x.Active).HasDefaultValue(true);
        modelBuilder.Entity<User>().Property(x => x.Phone).HasMaxLength(20);

        // Cari
        modelBuilder.Entity<Cari>().HasIndex(x => x.Kod).IsUnique();
        modelBuilder.Entity<Cari>().Property(x => x.Ad).IsRequired().HasMaxLength(200);
        modelBuilder.Entity<Cari>().HasIndex(x => x.CreatedByUserId);

        // CariAdres
        modelBuilder.Entity<CariAdres>()
            .HasOne(x => x.Cari)
            .WithMany(x => x.Adresler)
            .HasForeignKey(x => x.CariId)
            .OnDelete(DeleteBehavior.Cascade);

        // Stok
        modelBuilder.Entity<Stok>().HasIndex(x => x.Kod).IsUnique();
        modelBuilder.Entity<Stok>().Property(x => x.Ad).IsRequired().HasMaxLength(200);

        modelBuilder.Entity<StokFiyat>()
            .HasIndex(x => new { x.StokId, x.FiyatListeNo, x.YururlukTarihi }).IsUnique();
        modelBuilder.Entity<StokFiyat>()
            .HasOne(x => x.Stok)
            .WithMany(x => x.Fiyatlar)
            .HasForeignKey(x => x.StokId);

        // Teklif
        modelBuilder.Entity<Teklif>().HasIndex(x => x.No).IsUnique();
        modelBuilder.Entity<Teklif>().HasIndex(x => x.CreatedByUserId);
        modelBuilder.Entity<TeklifKalem>()
            .HasOne(x => x.Teklif)
            .WithMany(x => x.Kalemler)
            .HasForeignKey(x => x.TeklifId);

        modelBuilder.Entity<TeklifKalem>().Property(x => x.Miktar).HasPrecision(18, 2);
        modelBuilder.Entity<TeklifKalem>().Property(x => x.BirimFiyat).HasPrecision(18, 2);
        modelBuilder.Entity<TeklifKalem>().Property(x => x.Tutar).HasPrecision(18, 2);
        modelBuilder.Entity<TeklifKalem>().Property(x => x.IskontoTutar).HasPrecision(18, 2);
        modelBuilder.Entity<TeklifKalem>().Property(x => x.KdvTutar).HasPrecision(18, 2);
        modelBuilder.Entity<TeklifKalem>().Property(x => x.GenelTutar).HasPrecision(18, 2);
    
        // TeklifSepet
        modelBuilder.Entity<TeklifSepet>().HasIndex(x => x.UserId);
        modelBuilder.Entity<TeklifSepetKalem>()
            .HasOne(x => x.Sepet)
            .WithMany(x => x.Kalemler)
            .HasForeignKey(x => x.SepetId);
        modelBuilder.Entity<TeklifSepetKalem>().Property(x => x.Miktar).HasPrecision(18, 2);

        modelBuilder.Entity<Tedarikci>().HasIndex(x => x.Ad);
        modelBuilder.Entity<TedarikciFiyat>().HasIndex(x => new { x.StokId, x.FiyatListeNo, x.GuncellemeTarihi });
        modelBuilder.Entity<TedarikciFiyat>().HasIndex(x => new { x.StokId, x.TedarikciId, x.FiyatListeNo }).IsUnique(false);
    }
}

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        // Local SQL Server Express varsayılan instance
        optionsBuilder.UseSqlServer("Server=localhost\\SQLEXPRESS;Database=TeklifApp;Trusted_Connection=True;TrustServerCertificate=True;");

        return new AppDbContext(optionsBuilder.Options);
    }
}

