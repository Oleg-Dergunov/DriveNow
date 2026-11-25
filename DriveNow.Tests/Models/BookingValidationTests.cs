using System.ComponentModel.DataAnnotations;

namespace DriveNow.Tests.Models;

public class BookingValidationTests
{
    [Fact]
    public void ValidBooking_HasNoValidationErrors()
    {
        Assert.Empty(Validate(CreateValidBooking()));
    }

    [Fact]
    public void StartDateInThePast_FailsValidation()
    {
        var booking = CreateValidBooking();
        booking.StartDate = DateTime.Today.AddDays(-1);

        Assert.Contains(Validate(booking), r => r.MemberNames.Contains(nameof(Booking.StartDate)));
    }

    [Fact]
    public void StartDateToday_PassesValidation()
    {
        var booking = CreateValidBooking();
        booking.StartDate = DateTime.Today;
        booking.EndDate = DateTime.Today.AddDays(1);

        Assert.Empty(Validate(booking));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void EndDateNotAfterStartDate_FailsValidation(int daysAfterStart)
    {
        var booking = CreateValidBooking();
        booking.EndDate = booking.StartDate.AddDays(daysAfterStart);

        Assert.Contains(Validate(booking), r => r.MemberNames.Contains(nameof(Booking.EndDate)));
    }

    [Fact]
    public void BookingWithoutCar_FailsValidation()
    {
        var booking = CreateValidBooking();
        booking.CarId = 0;

        Assert.Contains(Validate(booking), r => r.MemberNames.Contains(nameof(Booking.CarId)));
    }

    [Fact]
    public void BookingWithoutLocation_FailsValidation()
    {
        var booking = CreateValidBooking();
        booking.LocationId = 0;

        Assert.Contains(Validate(booking), r => r.MemberNames.Contains(nameof(Booking.LocationId)));
    }

    [Fact]
    public void Days_IsNumberOfDaysBetweenDates()
    {
        var booking = CreateValidBooking();
        booking.EndDate = booking.StartDate.AddDays(3);

        Assert.Equal(3, booking.Days);
    }

    [Theory]
    [InlineData(-2, 1, true)]
    [InlineData(0, 1, true)]
    [InlineData(-3, 0, false)]
    [InlineData(1, 3, false)]
    public void IsActive_IsTrueOnlyWhileTodayIsInsideBookingPeriod(int startOffset, int endOffset, bool expected)
    {
        var booking = CreateBooking(startOffset, endOffset);

        Assert.Equal(expected, booking.IsActive);
    }

    [Theory]
    [InlineData(1, 3, true)]
    [InlineData(0, 1, false)]
    [InlineData(-2, 1, false)]
    public void IsUpcoming_IsTrueOnlyForFutureBookings(int startOffset, int endOffset, bool expected)
    {
        var booking = CreateBooking(startOffset, endOffset);

        Assert.Equal(expected, booking.IsUpcoming);
    }

    private static Booking CreateValidBooking() => CreateBooking(1, 4);

    private static Booking CreateBooking(int startOffset, int endOffset) => new()
    {
        CarId = 1,
        LocationId = 1,
        CustomerName = "Test Customer",
        StartDate = DateTime.Today.AddDays(startOffset),
        EndDate = DateTime.Today.AddDays(endOffset)
    };

    private static List<ValidationResult> Validate(Booking booking)
    {
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(booking, new ValidationContext(booking), results, validateAllProperties: true);
        return results;
    }
}