using System.Text.Json.Serialization;

namespace RecipeExtractorBot.DiscordInteractions;

public record class InteractionResponse
{
    public InteractionCallbackType Type { get; init; }
    public Message? Data { get; init; }

    public InteractionResponse() { }

    [JsonConstructor]
    public InteractionResponse(InteractionCallbackType type, Message? data = null)
    {
        Type = type;
        Data = data;
    }
}

public enum InteractionCallbackType
{
    NONE,
    PONG = 1,
    CHANNEL_MESSAGE_WITH_SOURCE = 4,
    DEFERRED_CHANNEL_MESSAGE_WITH_SOURCE = 5,
    DEFERRED_UPDATE_MESSAGE = 6,
    UPDATE_MESSAGE = 7,
    APPLICATION_COMMAND_AUTOCOMPLETE_RESULT = 8,
    MODAL = 9,
    PREMIUM_REQUIRED = 10,
}
