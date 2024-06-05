
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;

namespace RecipeExtractorBot.VideoInformation;

public partial class YouTubeInformationService(IConfiguration configuration) : IVideoInformationService
{
    [GeneratedRegex(@"^https:\/\/(?:(?:www\.youtube\.com\/watch\?([^v]+=.+&)*v=)|(?:youtu\.be\/))(?<id>[^\?&]+).*$", RegexOptions.Compiled)]
    private static partial Regex VideoRegex();
    private static readonly Regex _movieRegex = VideoRegex();

    private readonly YouTubeService _youtubeService = new(new BaseClientService.Initializer()
    {
        ApiKey = configuration.GetValue<string>("YOUTUBE_API_KEY") ?? string.Empty,
        ApplicationName = configuration.GetValue<string>("YOUTUBE_APPLICATION_NAME") ?? string.Empty,
    });

    public static bool IsVideoURL(string text) => _movieRegex.IsMatch(text);

    public static string? GetVideoURL(string text) => _movieRegex.Match(text) is { Success: true } match ? match.Value : null;

    public static IEnumerable<string> GetVideoURLs(string text) => _movieRegex.Matches(text)
                                                                        .Where(m => m.Success)
                                                                        .Select(m => m.Value);

    public static string? GetVideoId(string url) => _movieRegex.Match(url) is { Success: true } match ? match.Groups["id"].Value : null;

    public static IEnumerable<string> GetVideoIds(string text) => _movieRegex.Matches(text)
                                                                        .Where(m => m.Success)
                                                                        .Select(m => m.Groups["id"].Value);

    public async ValueTask<IVideoInformation?> GetVideoInformation(string videoId)
    {
        var video = await GetVideo(videoId);
        if (video is null)
        {
            return null;
        }

        var channel = await GetChannel(video.Snippet.ChannelId);

        return new YouTubeVideoInformation(video, channel)
        {
            VideoId = videoId,
        };
    }

    private async ValueTask<Video?> GetVideo(string videoId, CancellationToken cancellationToken = default)
    {
        var videoRequest = _youtubeService.Videos.List("snippet");
        videoRequest.Id = videoId;
        videoRequest.MaxResults = 1;

        var videoResponse = await videoRequest.ExecuteAsync(cancellationToken);

        if (videoResponse.Items.Count == 0)
        {
            return null;
        }
        var video = videoResponse.Items[0];
        return video;
    }

    private async ValueTask<Channel?> GetChannel(string channelId, CancellationToken cancellationToken = default)
    {
        var channelRequest = _youtubeService.Channels.List("snippet");
        channelRequest.Id = channelId;
        channelRequest.MaxResults = 1;

        var channelResponse = await channelRequest.ExecuteAsync(cancellationToken);

        if (channelResponse.Items.Count == 0)
        {
            return null;
        }

        var channel = channelResponse.Items[0];

        return channel;
    }

}
