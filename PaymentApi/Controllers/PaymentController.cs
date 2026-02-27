using Microsoft.AspNetCore.Mvc;
using Payment.Application.Services;
using Payment.Domain.Entities;

namespace Payment.Api.Controllers
{

    [ApiController]
    [Route("api/v1/payments")]
    public class PaymentController : ControllerBase
    {

        private readonly PaymentService _paymentService;
        private readonly ILogger<PaymentController> _logger;

        public PaymentController(
            PaymentService paymentService,
            ILogger<PaymentController> logger)
        {
            _paymentService = paymentService;
            _logger = logger;
        }


        // ===============================
        // CREATE PAYMENT
        // ===============================

        [HttpPost]
        public async Task<IActionResult> CreatePayment(
            [FromBody] CreatePaymentRequest request,
            [FromHeader(Name = "Idempotency-Key")]
            string idempotencyKey)
        {

            if (string.IsNullOrEmpty(idempotencyKey))
            {
                return BadRequest(
                    "Header Idempotency-Key obrigatório");
            }


            try
            {

                var payment =
                    await _paymentService.CreatePayment(
                        request.OrderId,
                        request.Amount,
                        idempotencyKey);


                _logger.LogInformation(
                    "Payment created {PaymentId}",
                    payment.Id);


                return Created(
                    $"api/v1/payments/{payment.Id}",
                    payment);
            }

            catch (Exception ex)
            {

                _logger.LogError(ex,
                    "Error creating payment");


                return BadRequest(ex.Message);
            }

        }



        // ===============================
        // GET PAYMENT
        // ===============================

        [HttpGet("{id}")]
        public async Task<IActionResult> GetPayment(Guid id)
        {

            var payment =
                await _paymentService.GetPayment(id);

            if (payment == null)
                return NotFound();

            return Ok(payment);
        }



        // ===============================
        // GET ALL PAYMENTS
        // ===============================

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {

            var payments =
                await _paymentService.GetAllPayments();

            return Ok(payments);
        }



        // ===============================
        // PAYMENT STATUS
        // ===============================

        [HttpGet("{id}/status")]
        public async Task<IActionResult> Status(Guid id)
        {

            var payment =
                await _paymentService.GetPayment(id);

            if (payment == null)
                return NotFound();

            return Ok(new
            {
                payment.Id,
                payment.Status
            });
        }

    }
}