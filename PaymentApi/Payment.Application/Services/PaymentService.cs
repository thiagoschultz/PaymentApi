using Microsoft.EntityFrameworkCore;
using Payment.Api.Data;
using Payment.Api.Models;
using Payment.Infrastructure.Data;
using Polly;
using Polly.Retry;
using StackExchange.Redis;
using System.Text.Json;

namespace Payment.Api.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly DbInitializer _context;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IDatabase _redis;
        private readonly ILogger<PaymentService> _logger;

        private readonly AsyncRetryPolicy _retryPolicy;

        public PaymentService(
            DbInitializer context,
            IHttpClientFactory httpClientFactory,
            IConnectionMultiplexer redis,
            ILogger<PaymentService> logger)
        {
            _context = context;
            _httpClientFactory = httpClientFactory;
            _redis = redis.GetDatabase();
            _logger = logger;

            _retryPolicy = Policy
                .Handle<Exception>()
                .WaitAndRetryAsync(
                    3,
                    retry => TimeSpan.FromSeconds(retry * 2),
                    (exception, timeSpan, retryCount, context) =>
                    {
                        _logger.LogWarning(
                            exception,
                            "Retry {RetryCount}",
                            retryCount);
                    });
        }

        public async Task<PaymentResponse> CreatePaymentAsync(
            PaymentRequest request,
            string idempotencyKey)
        {
            _logger.LogInformation(
                "Starting payment OrderId={OrderId}",
                request.OrderId);

            // =========================
            // IDEMPOTÊNCIA
            // =========================

            var existing = await _redis.StringGetAsync(idempotencyKey);

            if (!existing.IsNullOrEmpty)
            {
                _logger.LogInformation(
                    "Idempotent request detected");

                return JsonSerializer.Deserialize<PaymentResponse>(
                    existing)!;
            }

            // =========================
            // TRANSAÇÃO
            // =========================

            using var transaction =
                await _context.Database.BeginTransactionAsync();

            var payment = new Payment
            {
                Id = Guid.NewGuid(),
                OrderId = request.OrderId,
                Amount = request.Amount,
                Currency = request.Currency,
                Status = PaymentStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            _context.Payments.Add(payment);

            await _context.SaveChangesAsync();

            // =========================
            // CHAMADA GATEWAY
            // =========================

            var client =
                _httpClientFactory.CreateClient("gateway");

            var gatewayResult =
                await _retryPolicy.ExecuteAsync(async () =>
                {
                    var response = await client.PostAsJsonAsync(
                        "/api/gateway/pay",
                        payment);

                    response.EnsureSuccessStatusCode();

                    return await response.Content
                        .ReadFromJsonAsync<GatewayResponse>();
                });

            payment.Status = gatewayResult!.Success
                ? PaymentStatus.Approved
                : PaymentStatus.Refused;

            await _context.SaveChangesAsync();

            // =========================
            // OUTBOX PATTERN
            // =========================

            var outbox = new OutboxEvent
            {
                Id = Guid.NewGuid(),
                EventType = "PaymentCreated",
                Payload = JsonSerializer.Serialize(payment),
                CreatedAt = DateTime.UtcNow
            };

            _context.OutboxEvents.Add(outbox);

            await _context.SaveChangesAsync();

            await transaction.CommitAsync();

            // =========================
            // RESPONSE
            // =========================

            var responseModel = new PaymentResponse
            {
                Id = payment.Id,
                OrderId = payment.OrderId,
                Amount = payment.Amount,
                Status = payment.Status.ToString(),
                CreatedAt = payment.CreatedAt
            };

            // =========================
            // CACHE IDEMPOTÊNCIA
            // =========================

            await _redis.StringSetAsync(
                idempotencyKey,
                JsonSerializer.Serialize(responseModel),
                TimeSpan.FromHours(24));

            _logger.LogInformation(
                "Payment finished Id={PaymentId} Status={Status}",
                payment.Id,
                payment.Status);

            return responseModel;
        }

        public async Task<PaymentResponse?> GetPaymentAsync(Guid id)
        {
            var payment = await _context.Payments
                .FirstOrDefaultAsync(x => x.Id == id);

            if (payment == null)
                return null;

            return new PaymentResponse
            {
                Id = payment.Id,
                OrderId = payment.OrderId,
                Amount = payment.Amount,
                Status = payment.Status.ToString(),
                CreatedAt = payment.CreatedAt
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
            var payment = await _context.Payments
                .FirstOrDefaultAsync(x => x.Id == id);

            if (payment == null)
                return false;

            if (payment.Status == PaymentStatus.Approved)
                return false;

            payment.Status = PaymentStatus.Cancelled;

            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Payment cancelled {PaymentId}",
                id);

            return true;
        }
    }
}