namespace DriveNow.Tests.Services;

public class AuthServiceTests
{
    private readonly AuthService _service = new();

    [Theory]
    [InlineData("admin", "Admin123!", AppUser.AdminRole)]
    [InlineData(" STAFF ", "Staff123!", AppUser.StaffRole)]
    public void FindUser_WithValidCredentials_ReturnsUserWithRole(string username, string password, string expectedRole)
    {
        var user = _service.FindUser(username, password);

        Assert.NotNull(user);
        Assert.Equal(expectedRole, user.Role);
    }

    [Theory]
    [InlineData("admin", "admin123!")]
    [InlineData("admin", "wrong")]
    [InlineData("unknown", "Admin123!")]
    public void FindUser_WithInvalidCredentials_ReturnsNull(string username, string password)
    {
        Assert.Null(_service.FindUser(username, password));
    }
}