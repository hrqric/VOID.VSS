using Microsoft.AspNetCore.Mvc;
using VOID.VSS.Application.Commands.Users;

namespace VOID.VSS.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    [HttpPost("insertUser")]
    public async Task<IActionResult> InsertMovement([FromServices] UserCommandHandler handler,
        [FromQuery] InsertUserCommand cmd, CancellationToken ct)
    {
        return Ok(await handler.InsertUserAsync(cmd, ct));
    }
}