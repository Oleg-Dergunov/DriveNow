using DriveNow.Models;

namespace DriveNow.Services;

public interface IBookingService
{
    Task<List<Booking>> GetAllAsync();
    Task<Booking?> GetByIdAsync(int id);
    Task CreateAsync(Booking booking);
    Task DeleteAsync(int id);
}