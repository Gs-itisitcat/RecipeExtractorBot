using Microsoft.Extensions.Configuration;

namespace RecipeExtractorBot.VideoInformation;

public interface IVideoInformationService
{
    /// <summary>
    /// Determines whether the specified text is a video URL.
    /// </summary>
    /// <param name="text">The text to check.</param>
    /// <returns>Returns true if the text is a video URL; otherwise, false.</returns>
    public abstract static bool IsVideoURL(string text);
    public static bool isValidUrl(string url)
    {
        return url switch
        {
            _ when YouTubeInformationService.IsVideoURL(url) => true,
            _ => false,
        };
    }

    /// <summary>
    /// Gets the video URL from the specified text.
    /// </summary>
    /// <param name="text">The text to get the video URL from.</param>
    /// <returns>Returns the video URL.</returns>
    /// <remarks>
    /// Returns null if the text is not a video URL.
    /// </remarks>
    public abstract static string? GetVideoURL(string text);

    /// <summary>
    /// Gets the video URLs from the specified text.
    /// </summary>
    /// <param name="text">The text to get the video URLs from.</param>
    /// <returns>Returns the video URLs.</returns>
    public abstract static IEnumerable<string> GetVideoURLs(string text);

    /// <summary>
    /// Gets the <see cref="IVideoInformation"/> object from the specified video URL.
    /// </summary>
    /// <param name="videoUrl">The video URL to get the video information from.</param>
    /// <returns>Returns the <see cref="IVideoInformation"/> object representing the video information of the specified video URL.</returns>
    public ValueTask<IVideoInformation?> GetVideoInformation(string videoUrl);
    /// <summary>
    /// Gets the <see cref="IVideoInformation"/> object from the specified video URL.
    /// </summary>
    /// <param name="url">The video URL to get the video information from.</param>
    /// <param name="configuration">The configuration to use.</param>
    /// <returns>Returns the <see cref="IVideoInformation"/> object representing the video information of the specified video URL.</returns>
    public static IVideoInformationService? GetInformationServiceOf(string url, IConfiguration configuration)
    {
        return url switch
        {
            _ when YouTubeInformationService.IsVideoURL(url) => new YouTubeInformationService(configuration),
            _ => null,
        };
    }
}
