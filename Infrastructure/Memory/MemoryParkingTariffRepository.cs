using CoreApp.Entities;
using CoreApp.Repositories;

namespace Infrastructure.Memory;

public class MemoryParkingTariffRepository : MemoryGenericRepository<ParkingTariff>, IParkingTariffRepository
{
    public MemoryParkingTariffRepository()
    {
        var tariff1 = new ParkingTariff
        {
            Id = Guid.NewGuid(),
            Name = "Standard",
            FreeParkingDuration = TimeSpan.FromMinutes(15),
            HourlyRate = 5.00m,
            DailyMaxRate = 50.00m,
            IsActive = true
        };
        _data.Add(tariff1.Id, tariff1);

        var tariff2 = new ParkingTariff
        {
            Id = Guid.NewGuid(),
            Name = "Premium",
            FreeParkingDuration = TimeSpan.FromMinutes(30),
            HourlyRate = 8.00m,
            DailyMaxRate = 80.00m,
            IsActive = false
        };
        _data.Add(tariff2.Id, tariff2);
    }

    public Task<ParkingTariff?> FindActiveAsync()
    {
        return Task.FromResult(_data.Values.FirstOrDefault(t => t.IsActive));
    }
}
