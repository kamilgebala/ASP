using System.Security.Claims;
using CoreApp.Authorization;
using CoreApp.Dto;
using CoreApp.Enums;
using CoreApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;

[ApiController]
[Route("api/employee")]
[Authorize(Policy = nameof(AppPolicies.ParkingEmployeeOnly))]
public class ParkingEmployeeController(IParkingEmployeeService service) : ControllerBase
{
    private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;
    private bool IsAdmin => User.IsInRole(UserRole.Administrator.ToString());

    [HttpGet("sessions/active")]
    [ProducesResponseType(typeof(IEnumerable<ActiveSessionDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetActiveSessions()
        => Ok(await service.GetActiveSessionsAsync());

    [HttpPost("sessions/entry")]
    [ProducesResponseType(typeof(ParkingSessionDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RegisterManualEntry([FromBody] ManualEntryDto dto)
    {
        var session = await service.RegisterManualEntryAsync(dto, UserId);
        return Created($"/api/employee/sessions/{session.SessionId}", session);
    }

    [HttpPost("sessions/{id:guid}/exit")]
    [ProducesResponseType(typeof(ParkingSessionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RegisterManualExit(Guid id, [FromBody] ManualExitDto dto)
        => Ok(await service.RegisterManualExitAsync(id, dto, UserId, IsAdmin));

    [HttpPost("sessions/{id:guid}/close-free")]
    [ProducesResponseType(typeof(ParkingSessionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CloseSessionFree(Guid id, [FromBody] string reason)
        => Ok(await service.CloseSessionFreeAsync(id, reason, UserId, IsAdmin));

    [HttpGet("sessions/search")]
    [ProducesResponseType(typeof(IEnumerable<ActiveSessionDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> SearchByLicensePlate([FromQuery] string plate)
        => Ok(await service.SearchByLicensePlateAsync(plate));
}
