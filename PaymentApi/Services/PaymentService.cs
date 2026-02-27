using Payment.Api.Data;
using Payment.Api.Models;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using System.Text.Json;

namespace Payment.Api.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly PaymentDbContext _context;
        private readonly IDatabase _redis;
        private readonly IHttpClientFactory _clientFactory;

        public PaymentService(
            PaymentDbContext context,
            IConnectionMultiplexer redis,
            IHttpClientFactory factory)
        {
            _context = context;
            _redis = redis.GetDatabase();
            _clientFactory = factory;
        }

        public async Task<PaymentResponse> CreatePaymentAsync(
            PaymentRequest request,
            string idempotencyKey)
        {

            var cache = await _redis.StringGetAsync(idempotencyKey);

            if (!cache.IsNullOrEmpty)
                return JsonSerializer.Deserialize<PaymentResponse>(cache)!;


            var payment = new Payment
            {
                Id = Guid.NewGuid(),
                OrderId = request.OrderId,
                Amount = request.Amount,
                Status = PaymentStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            _context.Payments.Add(payment);

            await _context.SaveChangesAsync();


            var client = _clientFactory.CreateClient("gateway");

            var result =
                await client.PostAsJsonAsync(
                    "/api/gateway/pay",
                    payment);

            payment.Status =
                result.IsSuccessStatusCode
                ? PaymentStatus.Approved
                : PaymentStatus.Refused;

            await _context.SaveChangesAsync();


            var response = new PaymentResponse
            {
                Id = payment.Id,
                OrderId = payment.OrderId,
                Amount = payment.Amount,
                Status = payment.Status.ToString(),
                CreatedAt = payment.CreatedAt
            };


            await _redis.StringSetAsync(
                idempotencyKey,
                JsonSerializer.Serialize(response));

            return response;
        }

        public async Task<PaymentResponse?> GetPaymentAsync(Guid id)
        {

            var p = await _context.Payments.FirstOrDefaultAsync(x => x.Id == id);

            if (p == null)
                return null;

            return new PaymentResponse
            {
                Id = p.Id,
                OrderId = p.OrderId,
                Amount = p.Amount,
                Status = p.Status.ToString(),
                CreatedAt = p.CreatedAt
            };
        }

        public async Task<IEnumerable<PaymentResponse>> GetPaymentsAsync()
        {
            return await _context.Payments
                .Select(p => new PaymentResponse
                {
                    Id = p.Id,
                    OrderId = p.OrderId,
                    Amount = p.Amount,
                    Status = p.Status.ToString(),
                    CreatedAt = p.CreatedAt
                })
                .ToListAsync();
        }

        public async Task<bool> CancelPaymentAsync(Guid id)
        {

            var payment =
                await _context.Payments.FindAsync(id);

            if (payment == null)
                return false;

            payment.Status = PaymentStatus.Cancelled;

            await _context.SaveChangesAsync();

            return true;
        }
    }
}