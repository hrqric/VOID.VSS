using Microsoft.AspNetCore.Mvc;
using VOID.VSS.Application.Commands.Address;
using VOID.VSS.Application.Queries.Address;

namespace VOID.VSS.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AddressController() : ControllerBase
{
    [HttpPost("insertAddress")]
    public async Task<IActionResult> InsertAddress([FromServices] AddressCommandHandler handler,
        [FromQuery] InsertAddressCommand command, CancellationToken ct)
    {
        return Ok(await handler.InsertAddressAsync(command, HttpContext, ct));
    }

    [HttpGet("getAddress")]
    public async Task<IActionResult> GetAddress([FromServices] AddressQueryHandler handler,
        [FromQuery] GetAddressQuery query, CancellationToken ct)
    {
        return Ok(await handler.GetAddressHandleAsync(query, ct));
    }
    
}