using AppCore.Models;
using AppCore.ValueObject;

namespace AppCore.Repositories;

public interface ICarRepository: IGenericRepository<Car>
{
    Task<Car?> FindByPlateNumber(string plateNumber);
}