using System.Text.Json.Serialization;

namespace RecipeExtractorBot.DiscordInteractions;

public record class ApplicationCommand
{
    public string? Id { get; init; }
    public required string Name { get; init; }
    public ApplicationCommandType Type { get; init; }
    public string? Description { get; init; }
    public IReadOnlyList<ApplicationCommandOption>? Options { get; init; }
    public string? GuildId { get; init; }
    public string? TargetId { get; init; }

    public ApplicationCommand() { }

    [JsonConstructor]
    public ApplicationCommand(string id, string name, ApplicationCommandType type, IReadOnlyList<ApplicationCommandOption>? options = null, string? guildId = null, string? targetId = null)
    {
        Id = id;
        Name = name;
        Type = type;
        Options = options;
        GuildId = guildId;
        TargetId = targetId;
    }

    /// <summary>
    /// The list of application commands that the bot can handle.
    /// </summary>
    /// <value></value>
    public static IReadOnlyList<ApplicationCommand> ApplicationCommands { get; } = [
        new()
        {
            Name = "recipe-help",
            Type = ApplicationCommandType.CHAT_INPUT,
            Description = "Show the help message for the Recipe Extractor Bot.",
        },
        new()
        {
            Name = "recipe",
            Type = ApplicationCommandType.CHAT_INPUT,
            Description = "Extract the recipe from a video URL.",
            Options = [
                new()
                {
                    Name = "url",
                    Type = ApplicationCommandOptionType.STRING,
                    Description = "The URL of the video you want to extract the recipe from.",
                    Required = true,
                }
            ],
        },
    ];
}

public record class ApplicationCommandOption
{
    public required string Name { get; init; }
    public ApplicationCommandOptionType Type { get; init; }
    public string? Description { get; init; }
    public bool? Required { get; init; }
    public string? Value { get; init; }

    public List<ApplicationCommandOption>? Options { get; init; }

    public ApplicationCommandOption() { }

    [JsonConstructor]
    public ApplicationCommandOption(string name, ApplicationCommandOptionType type, string? value = null, List<ApplicationCommandOption>? options = null)
    {
        Name = name;
        Type = type;
        Value = value;
        Options = options;
    }
}

public enum ApplicationCommandOptionType
{
    NONE,
    SUB_COMMAND = 1,
    SUB_COMMAND_GROUP = 2,
    STRING = 3,
    INTEGER = 4,
    BOOLEAN = 5,
    USER = 6,
    CHANNEL = 7,
    ROLE = 8,
    MENTIONABLE = 9,
    NUMBER = 10,
    ATTACHMENT = 11,
}


public enum ApplicationCommandType
{
    NONE,
    CHAT_INPUT = 1,
    USER = 2,
    MESSAGE = 3,
}
