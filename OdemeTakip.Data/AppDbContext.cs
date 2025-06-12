using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using OdemeTakip.Entities;

namespace OdemeTakip.Data
{
    public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
    {
        public DbSet<Company> Companies { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<PaymentSource> PaymentSources { get; set; }
        public DbSet<CariFirma> CariFirmalar { get; set; }
        public DbSet<KrediKarti> KrediKartlari { get; set; }
        public DbSet<Kredi> Krediler { get; set; }
        public DbSet<Cek> Cekler { get; set; }
        public DbSet<SabitGider> SabitGiderler { get; set; }
        public DbSet<GenelOdeme> GenelOdemeler { get; set; }
        public DbSet<Banka> Bankalar { get; set; }
        public DbSet<BankaHesabi> BankaHesaplari { get; set; }
        public DbSet<KrediKartiOdeme> KrediKartiOdemeleri { get; set; }
        public DbSet<KrediKartiHarcama> KrediKartiHarcamalari { get; set; } // 🆕 EKLENDİ
        public DbSet<DegiskenOdeme> DegiskenOdemeler { get; set; }
        public DbSet<DegiskenOdemeSablonu> DegiskenOdemeSablonlari { get; set; }
        public DbSet<KrediTaksit> KrediTaksitler { get; set; }
    }

    public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            var connectionString = "Server=10.0.0.1,1434;Database=OdemeTakip;User Id=sa;Password=Yeksun.1288;TrustServerCertificate=True";

            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            optionsBuilder.UseSqlServer(connectionString);

            return new AppDbContext(optionsBuilder.Options);
        }

    }
}
