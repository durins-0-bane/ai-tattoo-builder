using Microsoft.AspNetCore.Mvc;
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

    [HttpPost("chat")]
    public async Task<ActionResult<ChatResponse>> Chat([FromBody] ChatRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Message))
        {
            return BadRequest(new { error = "Message cannot be empty." });
        }

        var reply = await _agentService.ChatAsync(request.Message);
        
        return Ok(new ChatResponse(reply));
    }
}

public record ChatRequest(string Message);

public record ChatResponse(string Reply);