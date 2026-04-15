using System.ComponentModel;
using MediatR;
using Microsoft.SemanticKernel;
using TattooShop.Api.Features;
using TattooShop.Api.Models;
using TattooShop.Api.Services;

namespace TattooShop.Api.Plugins;

public class TattooPortfolioPlugin(IMediator mediator, ChatExecutionContext executionContext)
{
    private readonly IMediator _mediator = mediator;
    private readonly ChatExecutionContext _executionContext = executionContext;

    [KernelFunction("save_tattoo_design")]
    [Description("Saves a new AI-generated tattoo design to the user's portfolio. Call this immediately after an image is successfully generated.")]
    public async Task<string> SaveDesignAsync(
        string imageUrl, 
        string prompt,   
        [Description("The detailed, technical prompt actually sent to the image generator.")] string refinedPrompt,
        [Description("The closest foundational art style. MUST be exactly one of: Traditional, Realism, Watercolor, Geometric, Blackwork, Neo-Traditional, or Custom/Hybrid. Put the weird, highly specific style details in the refinedPrompt instead.")] string style)
    {
        if (string.IsNullOrWhiteSpace(_executionContext.UserId))
        {
            return "Unable to save the design because no authenticated user context is available.";
        }

        var design = new TattooDesign(
            Id: Guid.NewGuid().ToString(),
            CustomerId: _executionContext.UserId,
            ImageUrl: imageUrl,
            Prompt: prompt,
            RefinedPrompt: refinedPrompt,
            Style: style,
            ArtistId: _executionContext.ArtistId,
            SessionId: _executionContext.SessionId,
            CreatedAt: DateTime.UtcNow,
            IsFavorite: false
        );

        await _mediator.Send(new SaveTattooDesignCommand(design));
        
        return $"Successfully saved design {design.Id} for customer {_executionContext.UserId}.";
    }
}