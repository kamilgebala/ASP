using CoreApp.Entities;

namespace CoreApp.Repositories;

public interface IParkingSessionRepository : IGenericRepositoryAsync<ParkingSession>
{
    Task<ParkingSession?> FindByLicensePlateAsync(string licensePlate);
    Task<IEnumerable<ParkingSession>> FindAllActiveAsync();
    Task<IEnumerable<ParkingSession>> FindHistoryByLicensePlateAsync(string licensePlate);
}
