using CoreApp.Entities;

namespace CoreApp.Repositories;

public interface IParkingGateRepository : IGenericRepositoryAsync<ParkingGate>
{
    Task<ParkingGate?> FindByNameAsync(string name);
    Task<ParkingGate?> FindByIdWithCapturesAsync(Guid id);
}