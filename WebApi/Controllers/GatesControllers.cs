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

    [HttpPost]
    public async Task<IActionResult> CreateGate([FromBody] CreateGateDto dto)
    {
        var created = await service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateGate(Guid id, [FromBody] UpdateGateDto dto)
    {
        var gate = await service.GetByIdAsync(id);
        if (gate is null)
            return NotFound();
        var updated = await service.UpdateAsync(id, dto);
        return Ok(updated);
    }

    [HttpPatch("{id:guid}/status")]
    public async Task<IActionResult> ChangeStatus(Guid id, [FromQuery] bool isOperational)
    {
        var updated = await service.ChangeOperationalStatusAsync(id, isOperational);
        return Ok(updated);
    }
}