using System.Text.Json.Serialization;

namespace RecipeExtractorBot.DiscordInteractions;

[method: JsonConstructor]
public class FollowUpResponse(string content = "", IReadOnlyList<Embed>? embeds = null, MessageFlags flags = 0)
{
    public string Content { get; init; } = content;
    public IReadOnlyList<Embed> Embeds { get; init; } = embeds ?? [];
    public MessageFlags Flags { get; init; } = flags;
}
