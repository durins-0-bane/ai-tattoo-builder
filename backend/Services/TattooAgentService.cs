using MediatR;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
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
    }

    public async Task<string> ChatAsync(string userMessage)
    {
        var history = new ChatHistory(
            "You are an AI assistant for a tattoo shop. " +
            "If the user asks to save a design, use your Portfolio plugin to save it. " +
            "If you don't know the CustomerId, just use 'demo-user-123'."
        );
        
        history.AddUserMessage(userMessage);

        var executionSettings = new OpenAIPromptExecutionSettings 
        { 
            ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions 
        };

        var result = await _chatCompletionService.GetChatMessageContentAsync(
            history, 
            executionSettings: executionSettings, 
            kernel: _kernel
        );

        return result.Content ?? "Sorry, I misplaced my ink. Mind trying that again?";
    }
}