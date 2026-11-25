using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace DriveNow.Models;

public class Booking : IValidatableObject
{
    public int Id { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Please select a car.")]
    public int CarId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Please select a pickup location.")]
    public int LocationId { get; set; }

    [Required(ErrorMessage = "Customer name is required.")]
    [StringLength(100, ErrorMessage = "Customer name cannot exceed 100 characters.")]
    public string CustomerName { get; set; } = string.Empty;

    [JsonPropertyName("pickupDate")]
    public DateTime StartDate { get; set; } = DateTime.Today;

    [JsonPropertyName("returnDate")]
    public DateTime EndDate { get; set; } = DateTime.Today.AddDays(1);

    [JsonIgnore]
    public bool IsActive => StartDate.Date <= DateTime.Today && EndDate.Date > DateTime.Today;

    [JsonIgnore]
    public bool IsUpcoming => StartDate.Date > DateTime.Today;

    [JsonIgnore]
    public int Days => Math.Max(1, (EndDate.Date - StartDate.Date).Days);

    public IEnumerable<ValidationResult> Validate(ValidationContext context)
    {
        if (StartDate.Date < DateTime.Today)
        {
            yield return new ValidationResult("Start date cannot be in the past.", new[] { nameof(StartDate) });
        }

        if (EndDate.Date <= StartDate.Date)
        {
            yield return new ValidationResult("End date must be after the start date.", new[] { nameof(EndDate) });
        }
    }
}