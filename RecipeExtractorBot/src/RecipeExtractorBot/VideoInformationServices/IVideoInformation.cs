namespace RecipeExtractorBot.VideoInformationServices;

public interface IVideoInformation
{
    public string? Title { get; }
    public string? Description { get; }
    public string URL { get; }
    public string? Thumbnail { get; }
    public string? ChannelName { get; }
    public string ChannelURL { get; }
    public string? ChannelId { get; }
    public string? ChannelIcon { get; }
}
