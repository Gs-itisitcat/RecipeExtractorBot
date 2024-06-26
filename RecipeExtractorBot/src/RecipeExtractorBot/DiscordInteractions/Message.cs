﻿namespace RecipeExtractorBot.DiscordInteractions;

public record class Message
{
    public string? Id { get; init; }
    public string? Content { get; init; }
    public IReadOnlyList<Embed>? Embeds { get; init; }
    public MessageFlags? Flags { get; init; }
}

[Flags]
public enum MessageFlags
{
    CROSSPOSTED = 1 << 0,
    IS_CROSSPOST = 1 << 1,
    SUPPRESS_EMBEDS = 1 << 2,
    SOURCE_MESSAGE_DELETED = 1 << 3,
    URGENT = 1 << 4,
    HAS_THREAD = 1 << 5,
    EPHEMERAL = 1 << 6,
    LOADING = 1 << 7,
    FAILED_TO_MENTION_SOME_ROLES_IN_THREAD = 1 << 8,
    SUPPRESS_NOTIFICATIONS = 1 << 12,
    IS_VOICE_MESSAGE = 1 << 13,

}
