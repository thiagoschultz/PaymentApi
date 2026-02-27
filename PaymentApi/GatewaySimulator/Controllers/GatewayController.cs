using Microsoft.AspNetCore.Mvc;

[Route("gateway")]
public class GatewayController : ControllerBase
{

    [HttpPost]
    public IActionResult Pay()
    {

        Random r = new Random();

        if (r.Next(1, 10) < 3)
            return StatusCode(500);

        return Ok("Approved");
    }
}