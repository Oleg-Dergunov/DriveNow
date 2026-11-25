namespace DriveNow.Tests.Services;

public class RentalServiceTests
{
    private readonly FakeCarService _cars = new();
    private readonly FakeBookingService _bookings = new();
    private readonly FakeLocationService _locations = new();
    private readonly RentalService _service;

    public RentalServiceTests()
    {
        _cars.Cars.Add(new Car { Id = 1, RegistrationNumber = "241-D-10234", Make = "Toyota", Model = "Corolla", Available = true });
        _cars.Cars.Add(new Car { Id = 2, RegistrationNumber = "242-D-55871", Make = "Toyota", Model = "Yaris", Available = false });
        _locations.Locations.Add(new Location { Id = 1, Name = "Dublin Airport", City = "Dublin", County = "Dublin" });

        _service = new RentalService(_cars, _bookings, _locations);
    }

    [Theory]
    [InlineData(-1, 2)]
    [InlineData(3, 5)]
    public async Task DeleteCarAsync_WithActiveOrUpcomingBooking_ThrowsConflictAndKeepsCar(int startOffset, int endOffset)
    {
        _bookings.Bookings.Add(CreateBooking(1, startOffset, endOffset));

        var ex = await Assert.ThrowsAsync<ApiException>(() => _service.DeleteCarAsync(1));

        Assert.Equal(HttpStatusCode.Conflict, ex.StatusCode);
        Assert.Contains(_cars.Cars, c => c.Id == 1);
    }

    [Fact]
    public async Task DeleteCarAsync_WithOnlyPastBookings_DeletesCar()
    {
        _bookings.Bookings.Add(CreateBooking(1, -10, -5));

        await _service.DeleteCarAsync(1);

        Assert.DoesNotContain(_cars.Cars, c => c.Id == 1);
    }

    [Fact]
    public async Task CreateBookingAsync_ForCarOutOfService_ThrowsConflictWithoutCreating()
    {
        var ex = await Assert.ThrowsAsync<ApiException>(() => _service.CreateBookingAsync(CreateBooking(2, 1, 3)));

        Assert.Equal(HttpStatusCode.Conflict, ex.StatusCode);
        Assert.Empty(_bookings.Bookings);
    }

    [Fact]
    public async Task CreateBookingAsync_ForMissingCar_ThrowsNotFoundWithoutCreating()
    {
        var ex = await Assert.ThrowsAsync<ApiException>(() => _service.CreateBookingAsync(CreateBooking(999, 1, 3)));

        Assert.Equal(HttpStatusCode.NotFound, ex.StatusCode);
        Assert.Empty(_bookings.Bookings);
    }

    [Fact]
    public async Task CreateBookingAsync_ForAvailableCar_CreatesBooking()
    {
        await _service.CreateBookingAsync(CreateBooking(1, 1, 3));

        Assert.Single(_bookings.Bookings);
    }

    [Theory]
    [InlineData(true, null, null, true)]
    [InlineData(true, -1, 2, false)]
    [InlineData(true, 3, 5, true)]
    [InlineData(false, null, null, false)]
    public void IsCarAvailable_RequiresCarInServiceWithoutActiveBooking(
        bool inService, int? startOffset, int? endOffset, bool expected)
    {
        var car = new Car { Id = 1, Available = inService };
        var bookings = startOffset is null
            ? new List<Booking>()
            : new List<Booking> { CreateBooking(1, startOffset.Value, endOffset!.Value) };

        Assert.Equal(expected, _service.IsCarAvailable(car, bookings));
    }

    private static Booking CreateBooking(int carId, int startOffset, int endOffset) => new()
    {
        CarId = carId,
        LocationId = 1,
        CustomerName = "Test Customer",
        StartDate = DateTime.Today.AddDays(startOffset),
        EndDate = DateTime.Today.AddDays(endOffset)
    };
}