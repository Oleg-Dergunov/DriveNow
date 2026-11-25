using DriveNow.Models;

namespace DriveNow.Services;

public interface ILocationService
{
    Task<List<Location>> GetAllAsync();
    Task<Location?> GetByIdAsync(int id);
}