using DriveNow.Models;

namespace DriveNow.Services;

public interface IRentalService
{
    Task DeleteCarAsync(int carId);
    Task CreateBookingAsync(Booking booking);
    bool IsCarBooked(int carId, IEnumerable<Booking> bookings);
    bool IsCarAvailable(Car car, IEnumerable<Booking> bookings);
}