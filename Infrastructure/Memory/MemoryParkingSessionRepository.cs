using CoreApp.Entities;
using CoreApp.Repositories;

namespace Infrastructure.Memory;

public class MemoryParkingSessionRepository : MemoryGenericRepository<ParkingSession>, IParkingSessionRepository
{
    public Task<ParkingSession?> FindByLicensePlateAsync(string licensePlate)
    {
        var result = _data.Values
            .FirstOrDefault(s => s.IsActive && s.Vehicle.LicensePlate.Equals(licensePlate, StringComparison.OrdinalIgnoreCase));
        return Task.FromResult(result);
    }

    public Task<IEnumerable<ParkingSession>> FindAllActiveAsync()
    {
        var result = _data.Values.Where(s => s.IsActive).AsEnumerable();
        return Task.FromResult(result);
    }

    public Task<IEnumerable<ParkingSession>> FindHistoryByLicensePlateAsync(string licensePlate)
    {
        var result = _data.Values
            .Where(s => s.Vehicle.LicensePlate.Equals(licensePlate, StringComparison.OrdinalIgnoreCase))
            .AsEnumerable();
        return Task.FromResult(result);
    }
}