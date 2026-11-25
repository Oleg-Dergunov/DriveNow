using System.Security.Claims;
using DriveNow.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace DriveNow.Services;

public class AuthService : IAuthService
{
    private static readonly List<(AppUser User, string Password)> Users = new()
    {
        (new AppUser("admin", "Administrator", AppUser.AdminRole), "Admin123!"),
        (new AppUser("staff", "Staff Member", AppUser.StaffRole), "Staff123!")
    };

    public AppUser? FindUser(string username, string password) =>
        Users.FirstOrDefault(u =>
            string.Equals(u.User.Username, username.Trim(), StringComparison.OrdinalIgnoreCase) &&
            u.Password == password).User;

    public Task SignInAsync(HttpContext httpContext, AppUser user)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, user.Username),
            new(ClaimTypes.GivenName, user.DisplayName),
            new(ClaimTypes.Role, user.Role)
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

        return httpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));
    }

    public Task SignOutAsync(HttpContext httpContext) =>
        httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
}