namespace DriveNow.Tests.Fakes;

public class FakeCarService : ICarService
{
    public List<Car> Cars { get; } = new();

    public Task<List<Car>> GetAllAsync() => Task.FromResult(Cars.ToList());

    public Task<Car?> GetByIdAsync(int id) => Task.FromResult(Cars.FirstOrDefault(c => c.Id == id));

    public Task CreateAsync(Car car)
    {
        Cars.Add(car);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(int id, Car car)
    {
        var index = Cars.FindIndex(c => c.Id == id);

        if (index >= 0)
        {
            Cars[index] = car;
        }

        return Task.CompletedTask;
    }

    public Task DeleteAsync(int id)
    {
        Cars.RemoveAll(c => c.Id == id);
        return Task.CompletedTask;
    }
}