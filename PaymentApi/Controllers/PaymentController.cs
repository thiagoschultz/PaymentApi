using Microsoft.AspNetCore.Mvc;
using Payment.Api.Models;
using Payment.Api.Services;

namespace Payment.Api.Controllers
{
    [ApiController]
    [Route("api/payments")]
    public class PaymentController : ControllerBase
    {

        private readonly IPaymentService _service;

        public PaymentController(IPaymentService service)
        {
            _service = service;
        }


        [HttpPost]
        public async Task<IActionResult> Create(
            PaymentRequest request,
            [FromHeader(Name = "Idempotency-Key")] string key)
        {
            var result =
                await _service.CreatePaymentAsync(request, key);

            return Ok(result);
        }


        [HttpGet("{id}")]
        public async Task<IActionResult> Get(Guid id)
        {
            var p = await _service.GetPaymentAsync(id);

            if (p == null)
                return NotFound();

            return Ok(p);
        }
    }
}