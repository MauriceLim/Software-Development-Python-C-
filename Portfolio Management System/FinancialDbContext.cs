using Microsoft.EntityFrameworkCore;
using MonteCarloSimulatorAPI.DataModels;

namespace MonteCarloSimulatorAPI.Data
{
    public class FinancialDbContext : DbContext
    {
        public DbSet<Exchange> Exchanges { get; set; }
        public DbSet<Market> Markets { get; set; }
        public DbSet<Underlying> Underlyings { get; set; }
        public DbSet<Derivative> Derivatives { get; set; }
        public DbSet<Trade> Trades { get; set; }
        public DbSet<Curve> Curves { get; set; }
        public DbSet<Rate> Rates { get; set; }
        public DbSet<Price> Prices { get; set; }

        public FinancialDbContext(DbContextOptions<FinancialDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Market to Exchange relationship
            modelBuilder.Entity<Market>()
                .HasOne(m => m.Exchange)
                .WithMany(e => e.Markets)
                .HasForeignKey(m => m.ExchangeID)
                .IsRequired();

            // Underlying to Market relationship
            modelBuilder.Entity<Underlying>()
                .HasOne(u => u.Market)
                .WithMany(m => m.Underlyings)
                .HasForeignKey(u => u.MarketID)
                .IsRequired();

            // Derivative to Underlying relationship
            modelBuilder.Entity<Derivative>()
                .HasOne(d => d.Underlying)
                .WithMany(u => u.Derivatives)
                .HasForeignKey(d => d.UnderlyingID)
                .IsRequired();

            // Price to Underlying relationship
            modelBuilder.Entity<Price>()
                .HasOne(p => p.Underlying)
                .WithMany(u => u.Prices)
                .HasForeignKey(p => p.UnderlyingID)
                .IsRequired();

            // Rate to Curve relationship
            modelBuilder.Entity<Rate>()
                .HasOne(r => r.Curve)
                .WithMany(c => c.Rates)
                .HasForeignKey(r => r.CurveID)
                .IsRequired();
        }
    }
}
