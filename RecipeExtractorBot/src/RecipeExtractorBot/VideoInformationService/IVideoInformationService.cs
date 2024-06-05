namespace RecipeExtractorBot.VideoInformation;

public interface IVideoInformationService
{
    /// <summary>
    /// Determines whether the specified text is a video URL.
    /// </summary>
    /// <param name="text">The text to check.</param>
    /// <returns>Returns true if the text is a video URL; otherwise, false.</returns>
    public abstract static bool IsVideoURL(string text);

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
    /// Gets the <see cref="IVideoInformation"/> object from the specified video ID.
    /// </summary>
    /// <param name="videoId">The video ID to get the video information from.</param>
    /// <returns>Returns the <see cref="IVideoInformation"/> object representing the video information of the specified video ID.</returns>
    public ValueTask<IVideoInformation?> GetVideoInformation(string videoId);
}
