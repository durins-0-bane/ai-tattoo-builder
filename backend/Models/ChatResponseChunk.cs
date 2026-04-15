namespace TattooShop.Api.Models;

public class ChatResponseChunk
{
    public string Text { get; set; } = string.Empty; 
    public string? ImageBase64 { get; set; } 
}