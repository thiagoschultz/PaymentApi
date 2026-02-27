using Microsoft.EntityFrameworkCore;
using Payment.Api.Models;

namespace Payment.Api.Data
{
    public class PaymentDbContext : DbContext
    {
        public PaymentDbContext(DbContextOptions<PaymentDbContext> options)
            : base(options)
        {
        }

        public DbSet<Payment> Payments => Set<Payment>();

        public DbSet<OutboxEvent> OutboxEvents => Set<OutboxEvent>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            modelBuilder.Entity<Payment>()
                .HasIndex(x => x.OrderId);

            modelBuilder.Entity<Payment>()
                .Property(x => x.Status)
                .HasConversion<string>();

            modelBuilder.Entity<Payment>()
                .Property(x => x.Amount)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<OutboxEvent>()
                .Property(x => x.Payload)
                .HasColumnType("jsonb");
        }
    }
}