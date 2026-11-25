using System.Net;
using DriveNow.Models;

namespace DriveNow.Services;

public class CarService : ApiServiceBase, ICarService
{
    private const string Endpoint = "cars";

    public CarService(HttpClient httpClient, ILogger<CarService> logger)
        : base(httpClient, logger)
    {
    }

    public async Task<List<Car>> GetAllAsync()
    {
        var cars = await GetFromApiAsync<List<Car>>(Endpoint, "load cars") ?? new List<Car>();
        cars.ForEach(Normalize);

        return cars
            .OrderBy(c => c.Make)
            .ThenBy(c => c.Model)
            .ToList();
    }

    public async Task<Car?> GetByIdAsync(int id)
    {
        try
        {
            var car = await GetFromApiAsync<Car>($"{Endpoint}/{id}", "load the car");

            if (car is not null)
            {
                Normalize(car);
            }

            return car;
        }
        catch (ApiException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task CreateAsync(Car car)
    {
        Normalize(car);
        await EnsureRegistrationIsUniqueAsync(car);

        await PostToApiAsync(Endpoint, car, "add the car", DuplicateRegistrationMessage(car));
    }

    public async Task UpdateAsync(int id, Car car)
    {
        Normalize(car);
        await EnsureRegistrationIsUniqueAsync(car);

        await PutToApiAsync($"{Endpoint}/{id}", car, "update the car", DuplicateRegistrationMessage(car));
    }

    public Task DeleteAsync(int id) =>
        DeleteFromApiAsync($"{Endpoint}/{id}", "delete the car", "This car has bookings and cannot be deleted.");

    private async Task EnsureRegistrationIsUniqueAsync(Car car)
    {
        var cars = await GetAllAsync();

        var isDuplicate = cars.Any(c =>
            c.Id != car.Id &&
            string.Equals(c.RegistrationNumber, car.RegistrationNumber, StringComparison.OrdinalIgnoreCase));

        if (isDuplicate)
        {
            throw new ApiException(DuplicateRegistrationMessage(car), HttpStatusCode.Conflict);
        }
    }

    private static string DuplicateRegistrationMessage(Car car) =>
        $"A car with registration number {car.RegistrationNumber} already exists.";

    private static void Normalize(Car car)
    {
        car.RegistrationNumber = car.RegistrationNumber?.Trim() ?? string.Empty;
        car.Make = car.Make?.Trim() ?? string.Empty;
        car.Model = car.Model?.Trim() ?? string.Empty;
        car.FuelType = car.FuelType?.Trim() ?? string.Empty;
    }
}