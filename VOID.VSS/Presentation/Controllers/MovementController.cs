using Microsoft.AspNetCore.Mvc;
using VOID.VSS.Application.Commands.Components.Stock.Movements;

namespace VOID.VSS.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MovementController : ControllerBase
{
    [HttpPost("insertMovement")]
    public async Task<IActionResult> InsertMovement([FromServices] MovementCommandHandler handler,
        [FromQuery] InsertMovementCommand command, CancellationToken ct)
    {
        return Ok(await handler.InsertMovementAsync(command, ct));
    }
}