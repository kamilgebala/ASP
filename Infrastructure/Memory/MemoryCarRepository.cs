using AppCore.Models;
using AppCore.Repositories;
using AppCore.ValueObject;

namespace Infrastructure.Memory;

public class MemoryCarRepository: ICarRepository
{
    public Task<Car?> FindById(int id)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<Car>> FindAll()
    {
        throw new NotImplementedException();
    }

    public Task<Car> Add(Car entity)
    {
        throw new NotImplementedException();
    }

    public Task DeleteById(int id)
    {
        throw new NotImplementedException();
    }

    public Task<Car> Update(Car entity)
    {
        throw new NotImplementedException();
    }

    public Task<Car?> FindByPlateNumber(string plateNumber)
    {
        if (plateNumber == "KR 1234T")
        {
            return Task.FromResult<Car?>(new Car()
            {
                Id = 1,
                PlateNumber = PlateNumber.Of(plateNumber),
                Entry = DateTime.Now,
                Exit = null
            });
        }

        return Task.FromResult<Car?>(null);
    }
}