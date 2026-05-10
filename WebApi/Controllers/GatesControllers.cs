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
        => Ok(await service.GetAllAsync(page, size));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var gate = await service.GetByIdAsync(id);
        return gate is null ? NotFound() : Ok(gate);
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
        if (gate is null) return NotFound();
        return Ok(await service.UpdateAsync(id, dto));
    }

    [HttpPatch("{id:guid}/status")]
    public async Task<IActionResult> ChangeStatus(Guid id, [FromQuery] bool isOperational)
        => Ok(await service.ChangeOperationalStatusAsync(id, isOperational));

    [HttpPost("{gateId:guid}/captures")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddCameraCapture(
        [FromRoute] Guid gateId,
        [FromBody] CreateCameraCaptureDto dto)
    {
        var capture = await service.AddCaptureAsync(gateId, dto);
        return CreatedAtAction(nameof(GetCaptures), new { gateId }, capture);
    }

    [HttpGet("{gateId:guid}/captures")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCaptures([FromRoute] Guid gateId)
        => Ok(await service.GetCapturesAsync(gateId));

    [HttpDelete("{gateId:guid}/captures/{captureId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveCapture([FromRoute] Guid gateId, [FromRoute] Guid captureId)
    {
        await service.RemoveCaptureAsync(gateId, captureId);
        return NoContent();
    }
}