using System.Text.Json.Serialization;

namespace RecipeExtractorBot.DiscordInteractions;

public record class Embed
{
    public string? Title { get; init; }
    public string? Description { get; init; }
    public string? Url { get; init; }
    public Author? Author { get; init; }
    public int? Color { get; init; }
    public EmbedImage? Image { get; init; }
    public List<Field>? Fields { get; init; }

    public Embed() { }

    [JsonConstructor]
    public Embed(string title = "", string description = "", string url = "", Author? author = null, int? color = null, EmbedImage? image = null, List<Field>? fields = null)
    {
        Title = title;
        Description = description;
        Url = url;
        Author = author;
        Color = color;
        Image = image;
        Fields = fields;
    }
}

public record class EmbedImage
{
    public string? Url { get; init; }
    public string? ProxyUrl { get; init; }
    public int? Height { get; init; }
    public int? Width { get; init; }

    public EmbedImage() { }

    [JsonConstructor]
    public EmbedImage(string url, string proxyUrl, int height, int width)
    {
        Url = url;
        ProxyUrl = proxyUrl;
        Height = height;
        Width = width;
    }
}

public record class Author
{
    public string Name { get; init; }
    public string? Url { get; init; }
    public string? IconUrl { get; init; }
    public string? ProxyIconUrl { get; init; }

    public Author(string name)
    {
        Name = name;
    }

    [JsonConstructor]
    public Author(string name, string url = "", string iconUrl = "", string proxyIconUrl = "")
    {
        Name = name;
        Url = url;
        IconUrl = iconUrl;
        ProxyIconUrl = proxyIconUrl;
    }
}

public record class Field
{
    public string Name { get; init; }
    public string Value { get; init; }
    public bool? Inline { get; set; }

    public Field(string name, string value)
    {
        Name = name;
        Value = value;
    }

    [JsonConstructor]
    public Field(string name, string value, bool inline = false)
    {
        Name = name;
        Value = value;
        Inline = inline;
    }
}

