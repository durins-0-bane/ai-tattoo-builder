namespace TattooShop.Api.Services;

public class ChatExecutionContext
{
    public string UserId { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
    public string UserDisplayName { get; set; } = string.Empty;
    public string ArtistId { get; set; } = string.Empty;
    public string SessionId { get; set; } = string.Empty;
    public string? GeneratedImageUrl { get; set; }
}
