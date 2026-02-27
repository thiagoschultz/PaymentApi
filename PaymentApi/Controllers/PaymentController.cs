using Microsoft.AspNetCore.Mvc;
using Payment.Application.Services;

[ApiController]
[Route("api/payments")]
public class PaymentController : ControllerBase
{
    private readonly PaymentService _service;

    public PaymentController(PaymentService service)
    {
        _service = service;
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromHeader] string IdempotencyKey,
        string orderId,
        decimal amount)
    {
        var payment =
            await _service.CreatePayment(
                orderId,
                amount,
                IdempotencyKey);

        return Ok(payment);
    }
}