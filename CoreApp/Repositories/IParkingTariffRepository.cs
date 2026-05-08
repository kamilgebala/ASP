using CoreApp.Entities;

namespace CoreApp.Repositories;

public interface IParkingTariffRepository : IGenericRepositoryAsync<ParkingTariff>
{
    Task<ParkingTariff?> FindActiveAsync();
}
