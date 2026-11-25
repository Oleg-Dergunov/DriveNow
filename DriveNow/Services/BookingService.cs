using System.Globalization;
using System.Net;
using DriveNow.Models;

namespace DriveNow.Services;

public class BookingService : ApiServiceBase, IBookingService
{
    private const string Endpoint = "bookings";

    public BookingService(HttpClient httpClient, ILogger<BookingService> logger)
        : base(httpClient, logger)
    {
    }

    public async Task<List<Booking>> GetAllAsync()
    {
        var bookings = await GetFromApiAsync<List<Booking>>(Endpoint, "load bookings") ?? new List<Booking>();
        bookings.ForEach(Normalize);

        return bookings
            .OrderBy(b => b.StartDate)
            .ToList();
    }

    public async Task<Booking?> GetByIdAsync(int id)
    {
        try
        {
            var booking = await GetFromApiAsync<Booking>($"{Endpoint}/{id}", "load the booking");

            if (booking is not null)
            {
                Normalize(booking);
            }

            return booking;
        }
        catch (ApiException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task CreateAsync(Booking booking)
    {
        Normalize(booking);

        var bookings = await GetAllAsync();
        var conflict = bookings.FirstOrDefault(b => b.CarId == booking.CarId && b.Id != booking.Id && Overlaps(b, booking));

        if (conflict is not null)
        {
            throw new ApiException(
                $"This car is already booked from {FormatDate(conflict.StartDate)} to {FormatDate(conflict.EndDate)}.",
                HttpStatusCode.Conflict);
        }

        await PostToApiAsync(Endpoint, booking, "create the booking", "This car is already booked for the selected dates.");
    }

    public Task DeleteAsync(int id) =>
        DeleteFromApiAsync($"{Endpoint}/{id}", "delete the booking");

    private static bool Overlaps(Booking first, Booking second) =>
        first.StartDate.Date < second.EndDate.Date && second.StartDate.Date < first.EndDate.Date;

    private static string FormatDate(DateTime date) =>
        date.ToString("dd MMM yyyy", CultureInfo.InvariantCulture);

    private static void Normalize(Booking booking)
    {
        booking.CustomerName = booking.CustomerName?.Trim() ?? string.Empty;
    }
}