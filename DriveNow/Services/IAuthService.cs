using DriveNow.Models;

namespace DriveNow.Services;

public interface IAuthService
{
    AppUser? FindUser(string username, string password);
    Task SignInAsync(HttpContext httpContext, AppUser user);
    Task SignOutAsync(HttpContext httpContext);
}