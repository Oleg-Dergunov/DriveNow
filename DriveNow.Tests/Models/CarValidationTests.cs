using System.ComponentModel.DataAnnotations;

namespace DriveNow.Tests.Models;

public class CarValidationTests
{
    [Fact]
    public void ValidCar_HasNoValidationErrors()
    {
        Assert.Empty(Validate(CreateValidCar()));
    }

    [Theory]
    [InlineData("ABCDE", 1990, 1)]
    [InlineData("ABCDEFGHIJKLMNOPQRST", 2035, 1000)]
    public void BoundaryValues_PassValidation(string registration, int year, decimal rate)
    {
        var car = CreateValidCar();
        car.RegistrationNumber = registration;
        car.Year = year;
        car.DailyRate = rate;

        Assert.Empty(Validate(car));
    }

    [Fact]
    public void EmptyCar_RequiresRegistrationMakeAndModel()
    {
        var invalidFields = Validate(new Car())
            .SelectMany(r => r.MemberNames)
            .Distinct()
            .OrderBy(name => name)
            .ToList();

        Assert.Equal(new[] { nameof(Car.Make), nameof(Car.Model), nameof(Car.RegistrationNumber) }, invalidFields);
    }

    [Theory]
    [InlineData("ABCD")]
    [InlineData("ABCDEFGHIJKLMNOPQRSTU")]
    public void RegistrationNumber_OutsideLengthLimits_FailsValidation(string registration)
    {
        var car = CreateValidCar();
        car.RegistrationNumber = registration;

        Assert.Contains(Validate(car), r => r.MemberNames.Contains(nameof(Car.RegistrationNumber)));
    }

    [Theory]
    [InlineData(1989)]
    [InlineData(2036)]
    public void Year_OutsideRange_FailsValidation(int year)
    {
        var car = CreateValidCar();
        car.Year = year;

        Assert.Contains(Validate(car), r => r.MemberNames.Contains(nameof(Car.Year)));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1001)]
    public void DailyRate_OutsideRange_FailsValidation(decimal rate)
    {
        var car = CreateValidCar();
        car.DailyRate = rate;

        Assert.Contains(Validate(car), r => r.MemberNames.Contains(nameof(Car.DailyRate)));
    }

    private static Car CreateValidCar() => new()
    {
        RegistrationNumber = "241-D-12345",
        Make = "Toyota",
        Model = "Corolla",
        Year = 2024,
        DailyRate = 55,
        FuelType = "Hybrid"
    };

    private static List<ValidationResult> Validate(Car car)
    {
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(car, new ValidationContext(car), results, validateAllProperties: true);
        return results;
    }
}