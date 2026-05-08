using CoreApp.Entities;

namespace CoreApp.Repositories;

public interface ICameraCaptureRepository : IGenericRepositoryAsync<CameraCapture>
{
    Task<IEnumerable<CameraCapture>> FindByLicensePlateAsync(string licensePlate);
}
