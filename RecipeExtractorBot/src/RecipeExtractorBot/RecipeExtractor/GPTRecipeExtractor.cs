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
        // Currently only supports YouTube
        return YouTubeInformationService.IsVideoURL(url);
    }
    public async ValueTask<(IVideoInformation? information, IReadOnlyList<Recipe> recipes)> ExtractRecipeAsync(string url, CancellationToken cancellationToken = default)
    {
        // 将来的には他の動画サイトにも対応する予定
#pragma warning disable CA1859 // 可能な場合は具象型を使用してパフォーマンスを向上させる
        IVideoInformationService? videoInformationService = url switch
        {
            _ when YouTubeInformationService.IsVideoURL(url) => new YouTubeInformationService(configuration),
            _ => null,
        };
#pragma warning restore CA1859 // 可能な場合は具象型を使用してパフォーマンスを向上させる

        if (videoInformationService is null)
        {
            return (null, []);
        }

        var videoId = YouTubeInformationService.GetVideoId(url);
        if (videoId is null)
        {
            return (null, []);
        }

        var videoInformation = await videoInformationService.GetVideoInformation(videoId);

        var description = videoInformation?.Description;
        if (string.IsNullOrWhiteSpace(description))
        {
            return (videoInformation, []);
        }

        var recipe = await _openAIService.ParseRecipeAsync(description, cancellationToken);
        return (videoInformation, recipe);
    }
}
