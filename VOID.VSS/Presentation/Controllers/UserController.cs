using Microsoft.AspNetCore.Mvc;
using VOID.VSS.Application.Commands.Users;

namespace VOID.VSS.Presentation.Controllers;

public class UserController : ControllerBase
{
    [ApiController]
    [Route("api/[controller]")]
    public class QuantityController : ControllerBase
    {
        [HttpGet("getQuantity")]
        public async Task<IActionResult> InsertMovement([FromServices] UserCommandHandler handler,
            [FromQuery] InsertUserCommand cmd, CancellationToken ct)
        {
            return Ok(await handler.InsertUserAsync(cmd, ct));
        }
    }
}