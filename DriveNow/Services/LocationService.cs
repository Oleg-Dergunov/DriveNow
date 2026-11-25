using System.Net;
using DriveNow.Models;

namespace DriveNow.Services;

public class LocationService : ApiServiceBase, ILocationService
{
    private const string Endpoint = "locations";

    public LocationService(HttpClient httpClient, ILogger<LocationService> logger)
        : base(httpClient, logger)
    {
    }

    public async Task<List<Location>> GetAllAsync()
    {
        var locations = await GetFromApiAsync<List<Location>>(Endpoint, "load locations") ?? new List<Location>();
        locations.ForEach(Normalize);

        return locations
            .OrderBy(l => l.Name)
            .ToList();
    }

    public async Task<Location?> GetByIdAsync(int id)
    {
        try
        {
            var location = await GetFromApiAsync<Location>($"{Endpoint}/{id}", "load the location");

            if (location is not null)
            {
                Normalize(location);
            }

            return location;
        }
        catch (ApiException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    private static void Normalize(Location location)
    {
        location.Name = location.Name?.Trim() ?? string.Empty;
        location.City = location.City?.Trim() ?? string.Empty;
        location.County = location.County?.Trim() ?? string.Empty;
    }
}