using CoreApp.Dto;
using CoreApp.Services;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GatesController(IParkingGateService service) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAllGates([FromQuery] int page = 1, [FromQuery] int size = 10)
    {
        return Ok(await service.GetAllAsync(page, size));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var gate = await service.GetByIdAsync(id);
        if (gate is null)
            return NotFound();
        return Ok(gate);
    }
}