namespace DriveNow.Tests.Fakes;

public class FakeBookingService : IBookingService
{
    public List<Booking> Bookings { get; } = new();

    public Task<List<Booking>> GetAllAsync() => Task.FromResult(Bookings.ToList());

    public Task<Booking?> GetByIdAsync(int id) => Task.FromResult(Bookings.FirstOrDefault(b => b.Id == id));

    public Task CreateAsync(Booking booking)
    {
        Bookings.Add(booking);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(int id)
    {
        Bookings.RemoveAll(b => b.Id == id);
        return Task.CompletedTask;
    }
}