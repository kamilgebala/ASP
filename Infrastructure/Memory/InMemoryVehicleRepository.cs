using CoreApp.Entities;
using CoreApp.Repositories;

namespace Infrastructure.Memory;

public class InMemoryVehicleRepository : MemoryGenericRepository<Vehicle>, IVehicleRepository
{
    public Task<Vehicle?> FindByLicensePlateAsync(string licensePlate)
    {
        var result = _data.Values
            .FirstOrDefault(v => v.LicensePlate.Equals(licensePlate, StringComparison.OrdinalIgnoreCase));

        return Task.FromResult(result);
    }
}
