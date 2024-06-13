using Microsoft.Extensions.Configuration;
using OpenAIService;
using RecipeExtractorBot.Serialization;
using RecipeExtractorBot.VideoInformation;

namespace RecipeExtractorBot.RecipeExtractor;

public class GPTRecipeExtractor(IOpenAIService openAIService, IConfiguration configuration) : IRecipeExtractor
{
    private readonly IOpenAIService _openAIService = openAIService;

    public bool IsSupported(string url)
    {
        return IVideoInformationService.isValidUrl(url);
    }
    public async ValueTask<(IVideoInformation? information, IReadOnlyList<Recipe> recipes)> ExtractRecipeAsync(string url, CancellationToken cancellationToken = default)
    {
        IVideoInformationService? videoInformationService = IVideoInformationService.GetInformationServiceOf(url, configuration);

        if (videoInformationService is null)
        {
            return (null, []);
        }

        var videoInformation = await videoInformationService.GetVideoInformation(url);

        var description = videoInformation?.Description;
        if (string.IsNullOrWhiteSpace(description))
        {
            return (videoInformation, []);
        }

        var recipe = await _openAIService.ParseRecipeAsync(description, cancellationToken);
        return (videoInformation, recipe);
    }
}
