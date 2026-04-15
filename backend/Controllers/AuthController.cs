using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TattooShop.Api.Repositories;
using TattooShop.Api.Services;

namespace TattooShop.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(IAuthService authService, IUserRepository userRepository) : ControllerBase
{
    [HttpPost("google")]
    public async Task<IActionResult> GoogleSignIn([FromBody] GoogleSignInRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.IdToken))
            return BadRequest(new { message = "ID token is required." });

        try
        {
            var result = await authService.AuthenticateWithGoogleAsync(request.IdToken);
            return Ok(new
            {
                token = result.Token,
                user = new
                {
                    id = result.User.Id,
                    email = result.User.Email,
                    displayName = result.User.DisplayName,
                    avatarUrl = result.User.AvatarUrl
                }
            });
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized(new { message = "Invalid Google token." });
        }
    }

    [Authorize]
    [HttpGet("/api/me")]
    public async Task<IActionResult> Me()
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("sub")?.Value;

        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized(new { message = "User ID claim missing." });
        }

        var user = await userRepository.GetByIdAsync(userId);
        if (user is null)
        {
            return NotFound(new { message = "User not found." });
        }

        return Ok(new { id = user.Id, email = user.Email, displayName = user.DisplayName, avatarUrl = user.AvatarUrl });
    }
}

public record GoogleSignInRequest(string IdToken);
