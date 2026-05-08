using CoreApp.Entities;
using CoreApp.Repositories;

namespace Infrastructure.Memory;

public class MemoryCameraCaptureRepository : MemoryGenericRepository<CameraCapture>, ICameraCaptureRepository
{
    public Task<IEnumerable<CameraCapture>> FindByLicensePlateAsync(string licensePlate)
    {
        return Task.FromResult(_data.Values.Where(c => c.LicensePlate == licensePlate));
    }
}
