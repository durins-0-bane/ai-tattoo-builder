using System.Text;
using System.Text.Json;
using MediatR;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using TattooShop.Api.Models;
using TattooShop.Api.Plugins;
using TattooShop.Api.Repositories;
using static TattooShop.Api.Plugins.TattooGeneratorPlugin;

namespace TattooShop.Api.Services;

public class TattooAgentService : ITattooAgentService
{
    private readonly IArtistProfileRepository _artistProfileRepository;
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IChatMessageRepository _chatMessageRepository;
    private readonly IChatSessionRepository _chatSessionRepository;
    private readonly IChatCompletionService _chatCompletionService;
    private readonly ChatExecutionContext _chatExecutionContext;
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;
    private readonly Kernel _kernel;
    private readonly ILogger<TattooAgentService> _logger;
    private readonly IMediator _mediator;

    public TattooAgentService(
        Kernel kernel,
        IMediator mediator,
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<TattooAgentService> logger,
        ILoggerFactory loggerFactory,
        IArtistProfileRepository artistProfileRepository,
        IAppointmentRepository appointmentRepository,
        IChatMessageRepository chatMessageRepository,
        IChatSessionRepository chatSessionRepository,
        ChatExecutionContext chatExecutionContext)
    {
        _kernel = kernel;
        _mediator = mediator;
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
        _artistProfileRepository = artistProfileRepository;
        _appointmentRepository = appointmentRepository;
        _chatMessageRepository = chatMessageRepository;
        _chatSessionRepository = chatSessionRepository;
        _chatExecutionContext = chatExecutionContext;
        _chatCompletionService = _kernel.GetRequiredService<IChatCompletionService>();

        _kernel.Plugins.AddFromObject(new TattooPortfolioPlugin(_mediator, _chatExecutionContext), "Portfolio");
        _kernel.Plugins.AddFromObject(new TattooGeneratorPlugin(_httpClient, _configuration, loggerFactory.CreateLogger<TattooGeneratorPlugin>()), "Generator");
        _kernel.Plugins.AddFromObject(new AppointmentPlugin(_mediator, _appointmentRepository, _chatExecutionContext), "Appointments");
    }

    public async Task<ChatReplyResponse> ChatAsync(
        string userId,
        string userEmail,
        string userDisplayName,
        string artistId,
        string? sessionId,
        string userMessage)
    {
        await _artistProfileRepository.EnsureSeedDataAsync();
        var artist = await _artistProfileRepository.GetByIdAsync(artistId);
        if (artist is null)
        {
            throw new InvalidOperationException($"Artist {artistId} was not found.");
        }

        var session = await GetOrCreateSessionAsync(userId, artistId, sessionId, userMessage);
        var existingMessages = await _chatMessageRepository.GetBySessionAsync(session.Id);

        _chatExecutionContext.UserId = userId;
        _chatExecutionContext.UserEmail = userEmail;
        _chatExecutionContext.UserDisplayName = userDisplayName;
        _chatExecutionContext.ArtistId = artist.Id;
        _chatExecutionContext.SessionId = session.Id;

        await _chatMessageRepository.AddAsync(new ChatMessage(
            Id: Guid.NewGuid().ToString(),
            SessionId: session.Id,
            UserId: userId,
            Role: "user",
            Content: userMessage,
            ImageBase64: null,
            CreatedAt: DateTime.UtcNow));

        var history = new ChatHistory(BuildSystemPrompt(artist, userDisplayName));
        foreach (var message in existingMessages)
        {
            if (message.Role == "user")
            {
                history.AddUserMessage(message.Content);
            }
            else if (message.Role == "assistant")
            {
                history.AddAssistantMessage(string.IsNullOrWhiteSpace(message.Content) ? "Generated image attached." : message.Content);
            }
        }

        history.AddUserMessage(userMessage);

        var executionSettings = new OpenAIPromptExecutionSettings
        {
            ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
        };

        var textBuilder = new StringBuilder();
        string? imageBase64 = null;

        var resultStream = _chatCompletionService.GetStreamingChatMessageContentsAsync(
            history,
            executionSettings: executionSettings,
            kernel: _kernel);

        await foreach (var chunk in resultStream)
        {
            if (!string.IsNullOrWhiteSpace(chunk.Content))
            {
                textBuilder.Append(chunk.Content);
            }
        }

        var lastToolMessage = history.LastOrDefault(m => m.Role == AuthorRole.Tool);
        if (lastToolMessage?.Content is { } toolContent && toolContent.Contains("Base64Data"))
        {
            try
            {
                var toolResult = JsonSerializer.Deserialize<ImageResult>(toolContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                imageBase64 = toolResult?.Base64Data;
            }
            catch (JsonException ex)
            {
                _logger.LogWarning(ex, "Failed to parse tool image result. Raw content: {Content}", toolContent);
            }
        }

        var responseText = string.IsNullOrWhiteSpace(textBuilder.ToString())
            ? "I mapped out a concept for you."
            : textBuilder.ToString().Trim();

        await _chatMessageRepository.AddAsync(new ChatMessage(
            Id: Guid.NewGuid().ToString(),
            SessionId: session.Id,
            UserId: userId,
            Role: "assistant",
            Content: responseText,
            ImageBase64: imageBase64,
            CreatedAt: DateTime.UtcNow));

        await _chatSessionRepository.UpsertAsync(session with { UpdatedAt = DateTime.UtcNow });
        return new ChatReplyResponse(session.Id, responseText, imageBase64);
    }

    private async Task<ChatSession> GetOrCreateSessionAsync(string userId, string artistId, string? sessionId, string userMessage)
    {
        if (!string.IsNullOrWhiteSpace(sessionId))
        {
            var existingSession = await _chatSessionRepository.GetByIdAsync(userId, sessionId);
            if (existingSession is not null)
            {
                return existingSession;
            }
        }

        var title = userMessage.Length > 48 ? $"{userMessage[..45]}..." : userMessage;
        var now = DateTime.UtcNow;
        var session = new ChatSession(
            Id: Guid.NewGuid().ToString(),
            UserId: userId,
            ArtistId: artistId,
            Title: title,
            CreatedAt: now,
            UpdatedAt: now);

        return await _chatSessionRepository.CreateAsync(session);
    }

    private static string BuildSystemPrompt(ArtistProfile artist, string userDisplayName) =>
        $"You are {artist.DisplayName}, an AI tattoo artist inside a showcase tattoo studio application. " +
        $"Your specialty areas are {string.Join(", ", artist.Specialties)}. " +
        $"Persona guidance: {artist.PersonaPrompt}\n" +
        $"Customer name: {userDisplayName}.\n" +
        "Directives:\n" +
        "- Keep answers practical, creative, and tattoo-aware.\n" +
        "- When a user asks for a concept or design, generate the image before summarizing it.\n" +
        "- Never claim a design was saved unless the save tool was actually called.\n" +
        "- Recommend placement, line weight, and longevity considerations where useful.\n" +
        "- If booking is requested, use the appointment tool.\n" +
        $"Artist bio: {artist.Bio}";
}