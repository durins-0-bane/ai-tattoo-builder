using MediatR;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.TextToImage;
using TattooShop.Api.Plugins;

namespace TattooShop.Api.Services;

public class TattooAgentService : ITattooAgentService
{
    private readonly Kernel _kernel;
    private readonly IChatCompletionService _chatCompletionService;

    public TattooAgentService(Kernel kernel, IMediator mediator)
    {
        _kernel = kernel;
        _chatCompletionService = _kernel.GetRequiredService<IChatCompletionService>();
        
        _kernel.Plugins.AddFromObject(new TattooPortfolioPlugin(mediator), pluginName: "Portfolio");
        var imageService = _kernel.GetRequiredService<ITextToImageService>();
        _kernel.Plugins.AddFromObject(new TattooGeneratorPlugin(imageService), "Generator");
    }

    public async IAsyncEnumerable<string> ChatStreamAsync(string userMessage)
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
                yield return chunk.Content;
            }
        }
    }
}