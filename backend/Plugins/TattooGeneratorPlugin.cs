using System.ComponentModel;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.SemanticKernel;

namespace TattooShop.Api.Plugins;

public class TattooGeneratorPlugin(HttpClient httpClient, IConfiguration config, ILogger<TattooGeneratorPlugin> logger)
{
    private readonly HttpClient _httpClient = httpClient;
    private readonly string _hfToken = config["HuggingFace:ApiKey"] ?? throw new ArgumentException("Hugging Face API Key missing!");
    private readonly ILogger<TattooGeneratorPlugin> _logger = logger;
    private static readonly string[] _retryTriggers = 
    [ 
        "currently loading", 
        "estimated_time", 
        "CUDA out of memory" 
    ];

    public record ImageResult(string MessageToAi, string? Base64Data = null);

    [KernelFunction("generate_tattoo_image")]
    [Description("Generates a high-quality tattoo image using the Stable Diffusion 3 Medium model.")]
    public async Task<ImageResult> GenerateImageAsync(
        [Description("The prompt describing the tattoo design.")] string refinedPrompt)
    {
        var modelId = "stabilityai/stable-diffusion-3-medium-diffusers";
        var url = $"https://router.huggingface.co/hf-inference/models/{modelId}";

        for (int attempt = 1; attempt <= 5; attempt++)
        {
            try 
            {
                using var request = new HttpRequestMessage(HttpMethod.Post, url);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _hfToken);
                
                var body = new { inputs = refinedPrompt };
                request.Content = JsonContent.Create(body);

                var response = await _httpClient.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    var imageBytes = await response.Content.ReadAsByteArrayAsync();
                    var base64 = Convert.ToBase64String(imageBytes);
                    
                    // THE ONLY CHANGE: Return a structured object
                    // We tell the AI "It's done", but we hide the raw data in a separate property
                    return new ImageResult(
                        MessageToAi: "The image has been generated successfully. Tell the user here is their design.", 
                        Base64Data: $"data:image/png;base64,{base64}"
                    );
                }

                var errorStr = await response.Content.ReadAsStringAsync();

                if (_retryTriggers.Any(errorStr.Contains))
                {
                    using var errorDoc = JsonDocument.Parse(errorStr);
                    
                    var estimatedTime = errorDoc.RootElement.TryGetProperty("estimated_time", out var timeElem) 
                        ? timeElem.GetDouble() 
                        : 15.0;

                    _logger.LogWarning("Model is loading. Retrying in {Time} seconds... (Attempt {Attempt}/5)", estimatedTime, attempt);
                    
                    await Task.Delay(TimeSpan.FromSeconds(estimatedTime));
                    continue; 
                }

                _logger.LogError("Hugging Face API Error {Code}: {Details}", response.StatusCode, errorStr);
                return new ImageResult($"Looks like my stencil printer jammed on that one. (Error {response.StatusCode})");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in Hugging Face plugin on attempt {Attempt}.", attempt);
                return new ImageResult("I snapped a needle trying to sketch that out. Let's wipe that down and try again in a moment.");
            }
        }

        return new ImageResult("My tattoo machine is taking a bit too long to tune. Let me adjust my coils, and when you're ready, ask me to draw that design again.");
    }
}