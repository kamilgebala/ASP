using CoreApp.Entities;
using CoreApp.Repositories;
using Infrastructure.EntityFramework.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.EntityFramework.Repositories;

public class EfParkingGateRepository(ParkingDbContext context)
    : EfGenericRepository<ParkingGate>(context.Gates), IParkingGateRepository
{
    public async Task<ParkingGate?> FindByNameAsync(string name)
        => await context.Gates
            .Include(g => g.CameraCaptures)
            .FirstOrDefaultAsync(g => g.Name == name);

    public async Task<ParkingGate?> FindByIdWithCapturesAsync(Guid id)
        => await context.Gates
            .Include(g => g.CameraCaptures)
            .FirstOrDefaultAsync(g => g.Id == id);
}