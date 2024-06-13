using RecipeExtractorBot.Serialization.DiscordInteraction.Embeds;
using RecipeExtractorBot.Serialization.DiscordInteraction.InteractionResponse;
using System.Text.Json.Serialization;

namespace RecipeExtractorBot.Serialization.DiscordInteraction.FollowUp;

[method: JsonConstructor]
public class FollowUpResponse(string content = "", Embed[]? embeds = null, MessageFlags flags = 0)
{
    public string Content { get; init; } = content;
    public IReadOnlyList<Embed> Embeds { get; init; } = embeds ?? [];
    public MessageFlags Flags { get; init; } = flags;
}
