using CoreApp.Entities;
using CoreApp.Repositories;
using Infrastructure.EntityFramework.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.EntityFramework.Repositories;

public class EfParkingTariffRepository(ParkingDbContext context)
    : EfGenericRepository<ParkingTariff>(context.Tariffs), IParkingTariffRepository
{
    public async Task<ParkingTariff?> FindActiveAsync()
        => await context.Tariffs.FirstOrDefaultAsync(t => t.IsActive);
}