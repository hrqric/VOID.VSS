using System.ComponentModel;
using Microsoft.AspNetCore.Mvc;
using VOID.VSS.Application.Commands.Address;
using VOID.VSS.Application.Commands.Components.Stock;

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
    
}