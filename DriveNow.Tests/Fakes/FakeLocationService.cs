namespace DriveNow.Tests.Fakes;

public class FakeLocationService : ILocationService
{
    public List<Location> Locations { get; } = new();

    public Task<List<Location>> GetAllAsync() => Task.FromResult(Locations.ToList());

    public Task<Location?> GetByIdAsync(int id) => Task.FromResult(Locations.FirstOrDefault(l => l.Id == id));
}