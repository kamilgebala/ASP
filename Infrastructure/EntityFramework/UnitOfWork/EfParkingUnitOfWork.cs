using CoreApp.Repositories;
using Infrastructure.EntityFramework.Context;
using Infrastructure.EntityFramework.Repositories;

namespace Infrastructure.EntityFramework.UnitOfWork;

public class EfParkingUnitOfWork(
    EfParkingGateRepository gates,
    EfParkingSessionRepository sessions,
    EfVehicleRepository vehicles,
    EfCameraCaptureRepository captures,
    EfParkingTariffRepository tariffs,
    ParkingDbContext context) : IParkingUnitOfWork
{
    public IVehicleRepository Vehicles => vehicles;
    public IParkingGateRepository Gates => gates;
    public IParkingSessionRepository Sessions => sessions;
    public ICameraCaptureRepository Captures => captures;
    public IParkingTariffRepository Tariffs => tariffs;

    public Task<int> SaveChangesAsync() => context.SaveChangesAsync();
    public Task BeginTransactionAsync() => context.Database.BeginTransactionAsync();
    public Task CommitTransactionAsync() => context.Database.CommitTransactionAsync();
    public Task RollbackTransactionAsync() => context.Database.RollbackTransactionAsync();
}