using CoreApp.Entities;
using CoreApp.Repositories;
using Infrastructure.EntityFramework.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.EntityFramework.Repositories;

public class EfParkingSessionRepository(ParkingDbContext context)
    : EfGenericRepository<ParkingSession>(context.Sessions), IParkingSessionRepository
{
    public override async Task<ParkingSession?> FindByIdAsync(Guid id)
        => await context.Sessions
            .Include(s => s.Vehicle)
            .FirstOrDefaultAsync(s => s.Id == id);

    public async Task<ParkingSession?> FindByLicensePlateAsync(string licensePlate)
        => await context.Sessions
            .Include(s => s.Vehicle)
            .Where(s => s.IsActive)
            .FirstOrDefaultAsync(s => s.Vehicle.LicensePlate == licensePlate);

    public async Task<IEnumerable<ParkingSession>> FindAllActiveAsync()
        => await context.Sessions
            .Include(s => s.Vehicle)
            .Where(s => s.IsActive)
            .ToListAsync();

    public async Task<IEnumerable<ParkingSession>> FindHistoryByLicensePlateAsync(string licensePlate)
        => await context.Sessions
            .Include(s => s.Vehicle)
            .Where(s => s.Vehicle.LicensePlate == licensePlate)
            .ToListAsync();
}