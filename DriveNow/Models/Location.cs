using System.Text.Json.Serialization;

namespace DriveNow.Models;

public class Location
{
    public int Id { get; set; }

    [JsonPropertyName("branchName")]
    public string Name { get; set; } = string.Empty;

    public string City { get; set; } = string.Empty;

    public string County { get; set; } = string.Empty;
}