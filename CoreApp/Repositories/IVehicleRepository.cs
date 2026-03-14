using CoreApp.Entities;

namespace CoreApp.Repositories;

public interface IVehicleRepository : IGenericRepositoryAsync<Vehicle>
{
    Task<Vehicle?> FindByLicensePlateAsync(string licensePlate);
}
