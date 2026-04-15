using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Google.Apis.Auth;
using Microsoft.IdentityModel.Tokens;
using TattooShop.Api.Models;
using TattooShop.Api.Repositories;

namespace TattooShop.Api.Services;

public class AuthService(IUserRepository userRepository, IConfiguration configuration) : IAuthService
{
    public async Task<AuthResult> AuthenticateWithGoogleAsync(string googleIdToken)
    {
        var clientId = configuration["Auth:Google:ClientId"]
            ?? throw new InvalidOperationException("Auth:Google:ClientId is not configured.");

        var settings = new GoogleJsonWebSignature.ValidationSettings
        {
            Audience = [clientId]
        };

        GoogleJsonWebSignature.Payload payload;
        try
        {
            payload = await GoogleJsonWebSignature.ValidateAsync(googleIdToken, settings);
        }
        catch (InvalidJwtException ex)
        {
            throw new UnauthorizedAccessException("Invalid Google ID token.", ex);
        }

        var user = await userRepository.GetByGoogleSubjectAsync(payload.Subject);

        var now = DateTime.UtcNow;
        user = user is null
            ? new User(
                Id: Guid.NewGuid().ToString(),
                GoogleSubject: payload.Subject,
                Email: payload.Email,
                DisplayName: payload.Name ?? payload.Email,
                AvatarUrl: payload.Picture,
                CreatedAt: now,
                LastLoginAt: now)
            : user with { DisplayName = payload.Name ?? user.DisplayName, AvatarUrl = payload.Picture, LastLoginAt = now };

        await userRepository.UpsertAsync(user);

        var token = CreateAppJwt(user);
        return new AuthResult(user, token);
    }

    private string CreateAppJwt(User user)
    {
        var secret = configuration["Auth:Jwt:Secret"]
            ?? throw new InvalidOperationException("Auth:Jwt:Secret is not configured.");
        var issuer = configuration["Auth:Jwt:Issuer"] ?? "TattooShop.Api";
        var audience = configuration["Auth:Jwt:Audience"] ?? "TattooShop.Client";
        var expiryHours = int.TryParse(configuration["Auth:Jwt:ExpiryHours"], out var h) ? h : 24;

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id),
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.Name, user.DisplayName),
            new Claim(ClaimTypes.Name, user.DisplayName),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(expiryHours),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
