namespace CoreApp.Repositories;

public interface IParkingUnitOfWork
{
    IVehicleRepository Vehicles { get; }
    IParkingGateRepository Gates { get; }
    IParkingSessionRepository Sessions { get; }
    ICameraCaptureRepository Captures { get; }
    IParkingTariffRepository Tariffs { get; }

    Task<int> SaveChangesAsync();
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}