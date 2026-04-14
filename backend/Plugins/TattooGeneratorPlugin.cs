using System.ComponentModel;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.TextToImage;

namespace TattooShop.Api.Plugins;

public class TattooGeneratorPlugin(ITextToImageService imageService)
{
    private readonly ITextToImageService _imageService = imageService;

    [KernelFunction("generate_tattoo_image")]
    [Description("Generates an image URL. Call this FIRST when a user requests a new design, before attempting to save.")]
    public async Task<string> GenerateImageAsync(
        [Description("Stricly format as [Subject] + [Style] + [Lighting/Composition].")] string refinedPrompt)
    {
        var imageUrl = await _imageService.GenerateImageAsync(refinedPrompt, 1024, 1024);
        
        return imageUrl;
    }
}