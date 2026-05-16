using CoreApp.Entities;
using CoreApp.Repositories;
using Infrastructure.EntityFramework.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.EntityFramework.Repositories;

public class EfVehicleRepository(ParkingDbContext context)
    : EfGenericRepository<Vehicle>(context.Vehicles), IVehicleRepository
{
    public async Task<Vehicle?> FindByLicensePlateAsync(string licensePlate)
        => await context.Vehicles
            .FirstOrDefaultAsync(v => v.LicensePlate == licensePlate);
}