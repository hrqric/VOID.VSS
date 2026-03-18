using System.ComponentModel;
using Microsoft.AspNetCore.Mvc;
using VOID.VSS.Application.Commands.Components.Stock;

namespace VOID.VSS.Presentation.Controllers;

[ApiController]
public class ComponentController() : ControllerBase
{
    [HttpPost("insertComponent")]
    public async Task<IActionResult> InsertComponent([FromServices] ComponentCommandHandler handler,
        [FromQuery] InsertComponentCommand command, CancellationToken ct)
    {
        return Ok(await handler.InsertComponentHandleAsync(command, HttpContext, ct));
    }
    
}