using MediatR;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using TattooShop.Api.Plugins;
using System.Text.Json;
using TattooShop.Api.Models;
using static TattooShop.Api.Plugins.TattooGeneratorPlugin;

namespace TattooShop.Api.Services;

public class TattooAgentService : ITattooAgentService
{
    private readonly Kernel _kernel;
    private readonly IChatCompletionService _chatCompletionService;
    private readonly ILogger<TattooGeneratorPlugin> _logger;

    public TattooAgentService(Kernel kernel, IMediator mediator, HttpClient httpClient, IConfiguration config,
        ILogger<TattooGeneratorPlugin> logger)
    {
        _kernel = kernel;
        _logger = logger;
        _chatCompletionService = _kernel.GetRequiredService<IChatCompletionService>();
        _kernel.Plugins.AddFromObject(new TattooPortfolioPlugin(mediator), "Portfolio");
        _kernel.Plugins.AddFromObject(new TattooGeneratorPlugin(httpClient, config, logger), "Generator");
    }

    public async IAsyncEnumerable<ChatResponseChunk> ChatStreamAsync(string userMessage)
    {
        var history = new ChatHistory(
            "Persona: A helpful, creative AI tattoo assistant.\n" +
            "Directives:\n" +
            "- NEVER save a design to the portfolio unless the user has explicitly approved the generated image first.\n" +
            "- When asked to create a design, ALWAYS generate the image URL before responding.\n" +
            "- Default customerId is 'demo-user-123'."
        );
        
        history.AddUserMessage(userMessage);

        var executionSettings = new OpenAIPromptExecutionSettings 
        { 
            ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions 
        };

        var resultStream = _chatCompletionService.GetStreamingChatMessageContentsAsync(
            history, 
            executionSettings: executionSettings, 
            kernel: _kernel
        );

        await foreach (var chunk in resultStream)
        {
            if (!string.IsNullOrEmpty(chunk.Content))
            {
                yield return new ChatResponseChunk { Text = chunk.Content };
            }
        }

        var lastToolMessage = history.LastOrDefault(m => m.Role == AuthorRole.Tool);
    
        if (lastToolMessage != null && lastToolMessage.Content != null && lastToolMessage.Content.Contains("Base64Data"))
        {
            ImageResult? toolResult = null;

            try 
            {
                toolResult = JsonSerializer.Deserialize<ImageResult>(
                    lastToolMessage.Content, 
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                );
            }
            catch (JsonException ex)
            {
                _logger?.LogWarning(ex, "Failed to parse ImageResult JSON from tool. Raw content: {Content}", lastToolMessage.Content);
            }

            if (!string.IsNullOrEmpty(toolResult?.Base64Data))
            {
                yield return new ChatResponseChunk { ImageBase64 = toolResult.Base64Data };
            }
        }
    }
}