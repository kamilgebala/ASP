using CoreApp.Dto;
using CoreApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;

[ApiController]
[Route("api/employee")]
[Authorize(Policy = "ParkingEmployeeOnly")]
public class ParkingEmployeeController(IParkingEmployeeService service) : ControllerBase
{
    [HttpGet("sessions/active")]
    [ProducesResponseType(typeof(IEnumerable<ActiveSessionDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetActiveSessions()
        => Ok(await service.GetActiveSessionsAsync());

    [HttpPost("sessions/entry")]
    [ProducesResponseType(typeof(ParkingSessionDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RegisterManualEntry([FromBody] ManualEntryDto dto)
    {
        var session = await service.RegisterManualEntryAsync(dto);
        return CreatedAtAction(nameof(GetActiveSessions), session);
    }

    [HttpPost("sessions/{id:guid}/exit")]
    [ProducesResponseType(typeof(ParkingSessionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RegisterManualExit(Guid id, [FromBody] ManualExitDto dto)
        => Ok(await service.RegisterManualExitAsync(id, dto));

    [HttpPost("sessions/{id:guid}/close-free")]
    [ProducesResponseType(typeof(ParkingSessionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CloseSessionFree(Guid id, [FromBody] string reason)
        => Ok(await service.CloseSessionFreeAsync(id, reason));

    [HttpGet("sessions/search")]
    [ProducesResponseType(typeof(IEnumerable<ActiveSessionDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> SearchByLicensePlate([FromQuery] string plate)
        => Ok(await service.SearchByLicensePlateAsync(plate));
}