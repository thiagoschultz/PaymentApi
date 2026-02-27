using Microsoft.AspNetCore.Mvc;
using Payment.Application.Services;

namespace Payment.Api.Controllers
{
    [ApiController]
    [Route("api/payments")]
    public class Woker : ControllerBase
    {
        private readonly PaymentService _service;

        public Woker(PaymentService service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<IActionResult> Create(
            string orderId,
            decimal amount)
        {
            var payment =
                await _service.CreatePayment(orderId, amount);

            return Ok(payment);
        }
    }
}