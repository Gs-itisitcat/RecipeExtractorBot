using RecipeExtractorBot.Recipes;
using RecipeExtractorBot.VideoInformationServices;

namespace RecipeExtractorBot.RecipeExtractor;

public interface IRecipeExtractor
{
    /// <summary>
    /// Determines whether the specified URL is supported.
    /// </summary>
    /// <param name="url">The URL to check.</param>
    /// <returns>Returns true if the URL is supported; otherwise, false.</returns>
    public bool IsSupported(string url);
    /// <summary>
    /// Extracts recipes from the specified URL.
    /// </summary>
    /// <param name="url">The URL to extract recipes from.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Returns the video information and extracted recipes.</returns>
    /// <remarks>
    /// Recipes will be null it failed to extract recipes, and video information will be null if the URL is not a video URL or failed to extract video information.
    public ValueTask<(IVideoInformation? information, IReadOnlyList<Recipe> recipes)> ExtractRecipeAsync(string url, CancellationToken cancellationToken = default);
}
