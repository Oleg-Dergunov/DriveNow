using DriveNow.Models;

namespace DriveNow.Services;

public interface ICarService
{
    Task<List<Car>> GetAllAsync();
    Task<Car?> GetByIdAsync(int id);
    Task CreateAsync(Car car);
    Task UpdateAsync(int id, Car car);
    Task DeleteAsync(int id);
}