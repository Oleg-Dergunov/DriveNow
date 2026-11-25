namespace DriveNow.Tests.Services;

public class CarServiceTests
{
    private const string CarsJson = """
        [
          {"id":1,"registrationNumber":"241-D-10234","make":"Toyota","model":"Corolla","year":2024,"dailyRate":55,"fuelType":"Petrol","available":true},
          {"id":5,"registrationNumber":"241-L-40112","make":"Hyundai","model":"Tucson","year":2024,"dailyRate":85,"fuelType":"Hybrid","available":true}
        ]
        """;

    [Theory]
    [InlineData("241-D-10234")]
    [InlineData(" 241-d-10234 ")]
    public async Task CreateAsync_WithExistingRegistration_ThrowsConflictWithoutPosting(string registration)
    {
        var (service, handler) = CreateService(_ => FakeHttpMessageHandler.Json(CarsJson));

        var ex = await Assert.ThrowsAsync<ApiException>(() => service.CreateAsync(CreateCar(registration)));

        Assert.Equal(HttpStatusCode.Conflict, ex.StatusCode);
        Assert.DoesNotContain(handler.Requests, r => r.Method == HttpMethod.Post);
    }

    [Fact]
    public async Task UpdateAsync_KeepingOwnRegistration_SendsPut()
    {
        var (service, handler) = CreateService(request => request.Method == HttpMethod.Get
            ? FakeHttpMessageHandler.Json(CarsJson)
            : new HttpResponseMessage(HttpStatusCode.NoContent));

        var car = CreateCar("241-D-10234");
        car.Id = 1;

        await service.UpdateAsync(1, car);

        Assert.Contains(handler.Requests, r => r.Method == HttpMethod.Put && r.RequestUri!.AbsolutePath.EndsWith("/cars/1"));
    }

    [Fact]
    public async Task DeleteAsync_WhenApiReturnsConflict_ThrowsFriendlyMessage()
    {
        var (service, _) = CreateService(_ => new HttpResponseMessage(HttpStatusCode.Conflict));

        var ex = await Assert.ThrowsAsync<ApiException>(() => service.DeleteAsync(1));

        Assert.Equal(HttpStatusCode.Conflict, ex.StatusCode);
        Assert.Equal("This car has bookings and cannot be deleted.", ex.Message);
    }

    [Fact]
    public async Task GetAllAsync_WhenServerIsUnreachable_ThrowsApiException()
    {
        var (service, _) = CreateService(_ => throw new HttpRequestException("No connection"));

        var ex = await Assert.ThrowsAsync<ApiException>(() => service.GetAllAsync());

        Assert.Null(ex.StatusCode);
    }

    private static Car CreateCar(string registration) => new()
    {
        RegistrationNumber = registration,
        Make = "Toyota",
        Model = "Corolla",
        Year = 2024,
        DailyRate = 55,
        FuelType = "Petrol"
    };

    private static (CarService Service, FakeHttpMessageHandler Handler) CreateService(
        Func<HttpRequestMessage, HttpResponseMessage> respond)
    {
        var handler = new FakeHttpMessageHandler(respond);
        return (new CarService(handler.CreateClient(), NullLogger<CarService>.Instance), handler);
    }
}