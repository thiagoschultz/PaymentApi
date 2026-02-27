using Microsoft.EntityFrameworkCore;
using Payment.Api.Models;

namespace Payment.Api.Data
{
    public class DbInitializer : DbContext
    {
        public DbInitializer(
            DbContextOptions<DbInitializer> options)
            : base(options)
        {
        }

        // =========================
        // TABELAS
        // =========================

        public DbSet<Payment> Payments { get; set; }

        public DbSet<OutboxEvent> OutboxEvents { get; set; }


        // =========================
        // MODEL CONFIGURATION
        // =========================

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Payment>(entity =>
            {
                entity.ToTable("payments");

                entity.HasKey(x => x.Id);

                entity.Property(x => x.OrderId)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(x => x.Amount)
                    .HasColumnType("decimal(18,2)");

                entity.Property(x => x.Currency)
                    .HasMaxLength(10);

                entity.Property(x => x.Status)
                    .HasConversion<string>();

                entity.Property(x => x.CreatedAt);

                entity.HasIndex(x => x.OrderId);
            });


            modelBuilder.Entity<OutboxEvent>(entity =>
            {
                entity.ToTable("outbox_events");

                entity.HasKey(x => x.Id);

                entity.Property(x => x.EventType)
                    .HasMaxLength(200);

                entity.Property(x => x.Payload)
                    .HasColumnType("jsonb");

                entity.Property(x => x.CreatedAt);

                entity.Property(x => x.ProcessedAt);

                entity.HasIndex(x => x.ProcessedAt);
            });
        }
    }
}