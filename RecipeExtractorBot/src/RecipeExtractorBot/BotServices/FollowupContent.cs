using System.Text.Json.Serialization;

namespace RecipeExtractorBot.BotServices;

public record class FollowupContent
{
    public string URL { get; init; }
    public string Token { get; init; }

    [JsonConstructor]
    public FollowupContent(string url, string token)
    {
        URL = url;
        Token = token;
    }
}
