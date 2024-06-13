using Google.Apis.YouTube.v3.Data;

namespace RecipeExtractorBot.VideoInformationServices;

public class YouTubeVideoInformation : IVideoInformation
{
    public required string VideoId { get; init; }
    private readonly Video? _video;
    private readonly Channel? _channel;

    internal YouTubeVideoInformation(Video? video = null, Channel? channel = null)
    {
        _video = video;
        _channel = channel;
    }

    public string? Title => _video?.Snippet?.Title;
    public string? Description => _video?.Snippet?.Description;
    public string URL => $"https://www.youtube.com/watch?v={VideoId}";
    public string? Thumbnail => _video?.Snippet?.Thumbnails?.Default__?.Url;
    public string? ChannelName => _channel?.Snippet?.Title;
    public string? ChannelId => _video?.Snippet?.ChannelId;
    public string? ChannelIcon => _channel?.Snippet?.Thumbnails?.Default__?.Url;
    public string ChannelURL => $"https://www.youtube.com/channel/{ChannelId}";

}
