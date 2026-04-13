using System.ComponentModel;
using MediatR;
using Microsoft.SemanticKernel;
using TattooShop.Api.Features;
using TattooShop.Api.Models;

namespace TattooShop.Api.Plugins;

public class TattooPortfolioPlugin(IMediator mediator)
{
    private readonly IMediator _mediator = mediator;

    [KernelFunction("save_tattoo_design")]
    [Description("Saves a new AI-generated tattoo design to the user's portfolio. Call this immediately after an image is successfully generated.")]
    public async Task<string> SaveDesignAsync(
        [Description("The unique ID of the customer.")] string customerId,
        string imageUrl, 
        string prompt,   
        [Description("The detailed, technical prompt actually sent to the image generator.")] string refinedPrompt,
        [Description("The closest foundational art style. MUST be exactly one of: Traditional, Realism, Watercolor, Geometric, Blackwork, Neo-Traditional, or Custom/Hybrid. Put the weird, highly specific style details in the refinedPrompt instead.")] string style)
    {
        var design = new TattooDesign(
            Id: Guid.NewGuid().ToString(),
            CustomerId: customerId,
            ImageUrl: imageUrl,
            Prompt: prompt,
            RefinedPrompt: refinedPrompt,
            Style: style,
            CreatedAt: DateTime.UtcNow,
            IsFavorite: false
        );

        await _mediator.Send(new SaveTattooDesignCommand(design));
        
        return $"Successfully saved design {design.Id} for customer {customerId}.";
    }
}