using System.ComponentModel;
using Microsoft.AspNetCore.Mvc;
using VOID.VSS.Application.Commands.Components.Stock;
using VOID.VSS.Application.Commands.Components.Stock.Stock.Queries;
using VOID.VSS.Application.Queries;

namespace VOID.VSS.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ComponentController() : ControllerBase
{
    [HttpPost("insertComponent")]
    public async Task<IActionResult> InsertComponent([FromServices] ComponentCommandHandler handler,
        [FromQuery] InsertComponentCommand command, CancellationToken ct)
    {
        return Ok(await handler.InsertComponentHandleAsync(command, HttpContext, ct));
    }
    
    [HttpGet("getComponent")]
    public async Task<IActionResult> GetComponent([FromServices] ComponentQueryHandler handler, [FromQuery] GetComponentQuery query, CancellationToken ct)
    {
        return Ok(await handler.GetComponentHandleAsync(query, ct));
    }
    
}