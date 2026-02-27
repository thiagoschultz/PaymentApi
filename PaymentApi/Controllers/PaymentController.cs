using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Payment.Api.Models;
using Payment.Api.Services;
using Payment.Application.Services;

namespace Payment.Api.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/payments")]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        private readonly ILogger<PaymentController> _logger;

        public PaymentController(
            IPaymentService paymentService,
            ILogger<PaymentController> logger)
        {
            _paymentService = paymentService;
            _logger = logger;
        }

        /// <summary>
        /// Criar pagamento
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreatePayment(
            [FromBody] PaymentRequest request,
            [FromHeader(Name = "Idempotency-Key")] string idempotencyKey)
        {
            if (string.IsNullOrEmpty(idempotencyKey))
                return BadRequest("Idempotency-Key header required");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            _logger.LogInformation(
                "Creating payment OrderId={OrderId} Amount={Amount}",
                request.OrderId,
                request.Amount);

            var result = await _paymentService.CreatePaymentAsync(
                request,
                idempotencyKey);

            return CreatedAtAction(
                nameof(GetPayment),
                new { id = result.Id },
                result);
        }

        /// <summary>
        /// Consultar pagamento
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetPayment(Guid id)
        {
            var payment = await _paymentService.GetPaymentAsync(id);

            if (payment == null)
                return NotFound();

            return Ok(payment);
        }

        /// <summary>
        /// Cancelar pagamento
        /// </summary>
        [HttpPost("{id}/cancel")]
        public async Task<IActionResult> CancelPayment(Guid id)
        {
            var success = await _paymentService.CancelPaymentAsync(id);

            if (!success)
                return NotFound();

            return Ok(new
            {
                message = "Payment cancelled"
            });
        }

        /// <summary>
        /// Listar pagamentos
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetPayments()
        {
            var payments = await _paymentService.GetPaymentsAsync();

            return Ok(payments);
        }

        /// <summary>
        /// Health endpoint financeiro
        /// </summary>
        [HttpGet("health")]
        public IActionResult Health()
        {
            return Ok(new
            {
                status = "UP",
                service = "Payment API",
                timestamp = DateTime.UtcNow
            });
        }
    }
}