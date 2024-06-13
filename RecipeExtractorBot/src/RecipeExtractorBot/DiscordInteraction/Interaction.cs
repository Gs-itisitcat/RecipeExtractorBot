using System.Text.Json.Serialization;
using RecipeExtractorBot.Serialization.DiscordInteraction.ApplicationCommands;

namespace RecipeExtractorBot.Serialization.DiscordInteraction.Interactions;

public record class Interaction
{
    public string Id { get; init; }
    public string ApplicationId { get; init; }
    public InteractionType Type { get; init; }
    public InteractionData? Data { get; init; }
    public string? GuildId { get; init; }
    public string? ChannelId { get; init; }
    public string Token { get; init; }

    [JsonConstructor]
    public Interaction(string id, string applicationId, InteractionType type, string token, InteractionData? data = null, string? guildId = null, string? channelId = null)
    {
        Id = id;
        ApplicationId = applicationId;
        Type = type;
        Data = data;
        GuildId = guildId;
        ChannelId = channelId;
        Token = token;
    }
}

public record class InteractionData
{
    public string Id { get; init; }
    public string Name { get; init; }
    public ApplicationCommandType Type { get; init; }
    public List<ApplicationCommandOption>? Options { get; init; }
    public string? GuildId { get; init; }
    public string? TargetId { get; init; }

    [JsonConstructor]
    public InteractionData(string id, string name, ApplicationCommandType type, List<ApplicationCommandOption>? options = null, string? guildId = null, string? targetId = null)
    {
        Id = id;
        Name = name;
        Type = type;
        Options = options;
        GuildId = guildId;
        TargetId = targetId;
    }


}

public enum InteractionType
{
    NONE,
    PING = 1,
    APPLICATION_COMMAND = 2,
    MESSAGE_COMPONENT = 3,
    APPLICATION_COMMAND_AUTOCOMPLETE = 4,
    MODAL_SUBMIT = 5,
}
