using System.Text.Json;

namespace DriveNow.Tests.Models;

public class ApiJsonMappingTests
{
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web);

    [Fact]
    public void Booking_ReadsPickupAndReturnDatesFromApi()
    {
        const string json = """
            {"id":2,"carId":5,"locationId":3,"customerName":"Aoife Murphy",
             "pickupDate":"2026-12-01T00:00:00","returnDate":"2026-12-05T00:00:00"}
            """;

        var booking = JsonSerializer.Deserialize<Booking>(json, Options)!;

        Assert.Equal(new DateTime(2026, 12, 1), booking.StartDate);
        Assert.Equal(new DateTime(2026, 12, 5), booking.EndDate);
        Assert.Equal(3, booking.LocationId);
    }

    [Fact]
    public void Booking_WritesOnlyApiFields()
    {
        var json = JsonSerializer.Serialize(new Booking { CarId = 1, LocationId = 2, CustomerName = "Test" }, Options);

        Assert.Equal(
            new[] { "carId", "customerName", "id", "locationId", "pickupDate", "returnDate" },
            GetPropertyNames(json));
    }

    [Fact]
    public void Car_WritesOnlyApiFields()
    {
        var json = JsonSerializer.Serialize(new Car(), Options);

        Assert.Equal(
            new[] { "available", "dailyRate", "fuelType", "id", "make", "model", "registrationNumber", "year" },
            GetPropertyNames(json));
    }

    [Fact]
    public void Location_ReadsBranchNameCityAndCounty()
    {
        const string json = """{"id":6,"branchName":"Kerry Airport","city":"Farranfore","county":"Kerry"}""";

        var location = JsonSerializer.Deserialize<Location>(json, Options)!;

        Assert.Equal("Kerry Airport", location.Name);
        Assert.Equal("Farranfore", location.City);
        Assert.Equal("Kerry", location.County);
    }

    private static string[] GetPropertyNames(string json)
    {
        using var document = JsonDocument.Parse(json);

        return document.RootElement
            .EnumerateObject()
            .Select(p => p.Name)
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToArray();
    }
}