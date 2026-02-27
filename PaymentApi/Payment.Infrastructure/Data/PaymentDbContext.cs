using Microsoft.EntityFrameworkCore;
using Payment.Domain.Entities;

namespace Payment.Infrastructure.Data
{
    public class PaymentDbContext : DbContext
    {

        public PaymentDbContext(DbContextOptions<PaymentDbContext> options)
            : base(options)
        {
        }


        // =============================
        // TABLES
        // =============================

        public DbSet<Payment> Payments { get; set; }

        public DbSet<OutboxMessage> Outbox { get; set; }

        public DbSet<PaymentEvent> PaymentEvents { get; set; }

        public DbSet<IdempotencyKey> IdempotencyKeys { get; set; }

        public DbSet<PaymentSagaState> SagaStates { get; set; }



        // =============================
        // MODEL CONFIGURATION
        // =============================

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            base.OnModelCreating(modelBuilder);


            // =============================
            // PAYMENT
            // =============================

            modelBuilder.Entity<Payment>(entity =>
            {

                entity.ToTable("Payments");

                entity.HasKey(x => x.Id);

                entity.Property(x => x.OrderId)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(x => x.Amount)
                    .HasColumnType("decimal(18,2)");

                entity.Property(x => x.Status)
                    .HasMaxLength(50);

                entity.Property(x => x.CreatedAt);


                entity.HasIndex(x => x.OrderId);

                entity.HasIndex(x => x.Status);

                entity.HasIndex(x => x.CreatedAt);

            });



            // =============================
            // OUTBOX
            // =============================

            modelBuilder.Entity<OutboxMessage>(entity =>
            {

                entity.ToTable("Outbox");

                entity.HasKey(x => x.Id);

                entity.Property(x => x.Type)
                    .HasMaxLength(100);

                entity.Property(x => x.Payload)
                    .HasColumnType("nvarchar(max)");

                entity.Property(x => x.Processed);

                entity.Property(x => x.CreatedAt);

                entity.HasIndex(x => x.Processed);

                entity.HasIndex(x => x.CreatedAt);

            });



            // =============================
            // EVENT STORE
            // =============================

            modelBuilder.Entity<PaymentEvent>(entity =>
            {

                entity.ToTable("PaymentEvents");

                entity.HasKey(x => x.Id);

                entity.Property(x => x.EventType)
                    .HasMaxLength(100);

                entity.Property(x => x.Data)
                    .HasColumnType("nvarchar(max)");

                entity.Property(x => x.CreatedAt);

                entity.HasIndex(x => x.PaymentId);

                entity.HasIndex(x => x.CreatedAt);

            });



            // =============================
            // IDEMPOTENCY
            // =============================

            modelBuilder.Entity<IdempotencyKey>(entity =>
            {

                entity.ToTable("IdempotencyKeys");

                entity.HasKey(x => x.Id);

                entity.Property(x => x.Key)
                    .HasMaxLength(200);

                entity.Property(x => x.CreatedAt);


                entity.HasIndex(x => x.Key)
                    .IsUnique();

            });



            // =============================
            // SAGA STATE
            // =============================

            modelBuilder.Entity<PaymentSagaState>(entity =>
            {

                entity.ToTable("PaymentSaga");

                entity.HasKey(x => x.Id);

                entity.Property(x => x.PaymentId);

                entity.Property(x => x.CurrentState)
                    .HasMaxLength(50);

                entity.Property(x => x.LastUpdated);


                entity.HasIndex(x => x.PaymentId);

            });

        }

    }
}