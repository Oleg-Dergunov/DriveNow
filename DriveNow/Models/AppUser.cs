namespace DriveNow.Models;

public record AppUser(string Username, string DisplayName, string Role)
{
    public const string AdminRole = "Admin";
    public const string StaffRole = "Staff";
}