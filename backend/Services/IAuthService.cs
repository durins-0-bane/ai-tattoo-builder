using TattooShop.Api.Models;

namespace TattooShop.Api.Services;

public record AuthResult(User User, string Token);

public interface IAuthService
{
    Task<AuthResult> AuthenticateWithGoogleAsync(string googleIdToken);
}
