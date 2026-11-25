namespace DriveNow.Tests.Services;

public class BookingServiceTests
{
    private const string BookingsJson = """
        [
          {"id":1,"carId":1,"locationId":1,"customerName":"John Smith",
           "pickupDate":"2030-06-10T00:00:00","returnDate":"2030-06-14T00:00:00"}
        ]
        """;

    [Theory]
    [InlineData(8, 11)]
    [InlineData(11, 13)]
    [InlineData(13, 16)]
    [InlineData(9, 15)]
    public async Task CreateAsync_OverlappingDatesForSameCar_ThrowsConflictWithoutPosting(int startDay, int endDay)
    {
        var (service, handler) = CreateService(RespondWithBookings);

        var ex = await Assert.ThrowsAsync<ApiException>(() => service.CreateAsync(CreateBooking(1, startDay, endDay)));

        Assert.Equal(HttpStatusCode.Conflict, ex.StatusCode);
        Assert.DoesNotContain(handler.Requests, r => r.Method == HttpMethod.Post);
    }

    [Theory]
    [InlineData(14, 16)]
    [InlineData(6, 10)]
    public async Task CreateAsync_BookingNextToExistingBooking_IsPosted(int startDay, int endDay)
    {
        var (service, handler) = CreateService(RespondWithBookings);

        await service.CreateAsync(CreateBooking(1, startDay, endDay));

        Assert.Contains(handler.Requests, r => r.Method == HttpMethod.Post);
    }

    [Fact]
    public async Task CreateAsync_SameDatesForDifferentCar_IsPosted()
    {
        var (service, handler) = CreateService(RespondWithBookings);

        await service.CreateAsync(CreateBooking(2, 10, 14));

        Assert.Contains(handler.Requests, r => r.Method == HttpMethod.Post);
    }

    private static HttpResponseMessage RespondWithBookings(HttpRequestMessage request) =>
        request.Method == HttpMethod.Get
            ? FakeHttpMessageHandler.Json(BookingsJson)
            : new HttpResponseMessage(HttpStatusCode.Created);

    private static Booking CreateBooking(int carId, int startDay, int endDay) => new()
    {
        CarId = carId,
        LocationId = 1,
        CustomerName = "Test Customer",
        StartDate = new DateTime(2030, 6, startDay),
        EndDate = new DateTime(2030, 6, endDay)
    };

    private static (BookingService Service, FakeHttpMessageHandler Handler) CreateService(
        Func<HttpRequestMessage, HttpResponseMessage> respond)
    {
        var handler = new FakeHttpMessageHandler(respond);
        return (new BookingService(handler.CreateClient(), NullLogger<BookingService>.Instance), handler);
    }
}