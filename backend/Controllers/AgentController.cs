using Microsoft.AspNetCore.Mvc;
using TattooShop.Api.Models;
using TattooShop.Api.Services;

namespace TattooShop.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AgentController : ControllerBase
{
    private readonly ITattooAgentService _agentService;

    public AgentController(ITattooAgentService agentService)
    {
        _agentService = agentService;
    }

    [HttpPost("chat-stream")]
    public async IAsyncEnumerable<ChatResponseChunk> ChatStream([FromBody] ChatRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Message))
        {
            yield return new ChatResponseChunk { Text = "Error: Message cannot be empty." };
            yield break; 
        }

        var stream = _agentService.ChatStreamAsync(request.Message);
        
        await foreach (var chunk in stream)
        {
            yield return chunk;
        }
    }
}

public record ChatRequest(string Message);