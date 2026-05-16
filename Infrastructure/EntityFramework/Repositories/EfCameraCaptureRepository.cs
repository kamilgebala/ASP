using CoreApp.Entities;
using CoreApp.Repositories;
using Infrastructure.EntityFramework.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.EntityFramework.Repositories;

public class EfCameraCaptureRepository(ParkingDbContext context)
    : EfGenericRepository<CameraCapture>(context.Captures), ICameraCaptureRepository
{
    public async Task<IEnumerable<CameraCapture>> FindByLicensePlateAsync(string licensePlate)
        => await context.Captures
            .Where(c => c.LicensePlate == licensePlate)
            .ToListAsync();
}