using Payment.Domain.Entities;
using Payment.Infrastructure.Data;
using System.Text.Json;

namespace Payment.Application.Services
{
    public class PaymentService
    {
        private readonly PaymentDbContext _context;

        public PaymentService(PaymentDbContext context)
        {
            _context = context;
        }

        public async Task<Payment> CreatePayment(
            string orderId,
            decimal amount,
            string idempotencyKey)
        {

            var exists = _context.IdempotencyKeys
                .Any(x => x.Key == idempotencyKey);

            if (exists)
                throw new Exception("Pagamento já processado");


            var payment = new Payment
            {
                Id = Guid.NewGuid(),
                OrderId = orderId,
                Amount = amount,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            };


            _context.Payments.Add(payment);

            _context.IdempotencyKeys.Add(
                new IdempotencyKey
                {
                    Id = Guid.NewGuid(),
                    Key = idempotencyKey,
                    CreatedAt = DateTime.UtcNow
                });

            _context.Outbox.Add(
                new OutboxMessage
                {
                    Id = Guid.NewGuid(),
                    Type = "PaymentCreated",
                    Payload = JsonSerializer.Serialize(payment),
                    CreatedAt = DateTime.UtcNow,
                    Processed = false
                });


            await _context.SaveChangesAsync();

            return payment;
        }
    }
}