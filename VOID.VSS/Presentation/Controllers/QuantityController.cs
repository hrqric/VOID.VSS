using Microsoft.AspNetCore.Mvc;
using VOID.VSS.Application.Commands.Components.Stock.Stock.Queries;
using VOID.VSS.Application.Queries;

namespace VOID.VSS.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
public class QuantityController : ControllerBase
{
    [HttpGet("getQuantity")]
    public async Task<IActionResult> InsertMovement([FromServices] QuantityQueryHandler handler,
        [FromQuery] GetQuantityQuery query, CancellationToken ct)
    {
        return Ok(await handler.GetQuantityHandleAsync(query, ct));
    }
}