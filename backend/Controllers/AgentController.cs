using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TattooShop.Api.Models;
using TattooShop.Api.Repositories;
using TattooShop.Api.Services;

namespace TattooShop.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AgentController(
    ITattooAgentService agentService,
    IChatSessionRepository chatSessionRepository,
    IChatMessageRepository chatMessageRepository) : ControllerBase
{
    private readonly ITattooAgentService _agentService = agentService;
    private readonly IChatSessionRepository _chatSessionRepository = chatSessionRepository;
    private readonly IChatMessageRepository _chatMessageRepository = chatMessageRepository;

    [Authorize]
    [HttpGet("sessions")]
    public async Task<ActionResult<IEnumerable<ChatSession>>> GetSessions()
    {
        var sessions = await _chatSessionRepository.GetByUserAsync(GetUserId());
        return Ok(sessions);
    }

    [Authorize]
    [HttpGet("sessions/{sessionId}/messages")]
    public async Task<ActionResult<IEnumerable<ChatMessage>>> GetMessages(string sessionId)
    {
        var session = await _chatSessionRepository.GetByIdAsync(GetUserId(), sessionId);
        if (session is null)
        {
            return NotFound(new { message = "Chat session not found." });
        }

        var messages = await _chatMessageRepository.GetBySessionAsync(sessionId);
        return Ok(messages);
    }

    [Authorize]
    [HttpPost("chat")]
    public async Task<ActionResult<ChatReplyResponse>> Chat([FromBody] ChatRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Message))
        {
            return BadRequest(new { message = "Message cannot be empty." });
        }

        if (string.IsNullOrWhiteSpace(request.ArtistId))
        {
            return BadRequest(new { message = "Artist ID is required." });
        }

        var result = await _agentService.ChatAsync(
            GetUserId(),
            GetUserEmail(),
            GetUserDisplayName(),
            request.ArtistId,
            request.SessionId,
            request.Message);

        return Ok(result);
    }

    private string GetUserId() =>
        User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? User.FindFirstValue("sub")
        ?? throw new UnauthorizedAccessException("User ID claim is missing.");

    private string GetUserEmail() =>
        User.FindFirstValue(ClaimTypes.Email)
        ?? User.FindFirstValue("email")
        ?? string.Empty;

    private string GetUserDisplayName() =>
        User.FindFirstValue(ClaimTypes.Name)
        ?? User.FindFirstValue("name")
        ?? "Tattoo Collector";
}

public record ChatRequest(string Message, string ArtistId, string? SessionId);