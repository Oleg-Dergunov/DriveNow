using System.Net;
using DriveNow.Models;

namespace DriveNow.Services;

public class RentalService : IRentalService
{
    private readonly ICarService _carService;
    private readonly IBookingService _bookingService;
    private readonly ILocationService _locationService;

    public RentalService(ICarService carService, IBookingService bookingService, ILocationService locationService)
    {
        _carService = carService;
        _bookingService = bookingService;
        _locationService = locationService;
    }

    public async Task DeleteCarAsync(int carId)
    {
        var bookings = await _bookingService.GetAllAsync();

        if (bookings.Any(b => b.CarId == carId && (b.IsActive || b.IsUpcoming)))
        {
            throw new ApiException(
                "This car has active or upcoming bookings and cannot be deleted.",
                HttpStatusCode.Conflict);
        }

        await _carService.DeleteAsync(carId);
    }

    public async Task CreateBookingAsync(Booking booking)
    {
        var car = await _carService.GetByIdAsync(booking.CarId)
            ?? throw new ApiException("The selected car no longer exists.", HttpStatusCode.NotFound);

        if (!car.Available)
        {
            throw new ApiException("This car is out of service and cannot be booked.", HttpStatusCode.Conflict);
        }

        _ = await _locationService.GetByIdAsync(booking.LocationId)
            ?? throw new ApiException("The selected pickup location no longer exists.", HttpStatusCode.NotFound);

        await _bookingService.CreateAsync(booking);
    }

    public bool IsCarBooked(int carId, IEnumerable<Booking> bookings) =>
        bookings.Any(b => b.CarId == carId && b.IsActive);

    public bool IsCarAvailable(Car car, IEnumerable<Booking> bookings) =>
        car.Available && !IsCarBooked(car.Id, bookings);
}