using Microsoft.EntityFrameworkCore;
using Payment.Domain.Entities;

namespace Payment.Infrastructure.Data
{
    public class PaymentDbContext : DbContext
    {
        public PaymentDbContext(DbContextOptions options)
            : base(options)
        {
        }

        public DbSet<Payment> Payments { get; set; }

        public DbSet<OutboxMessage> Outbox { get; set; }
        public DbSet<IdempotencyKey> IdempotencyKeys { get; set; }
    }
}