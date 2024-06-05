using System.Text.Json.Serialization;

namespace RecipeExtractorBot.Serialization;

public record class Ingredient
{
    public string Name { get; init; }
    public string? Amount { get; init; }
    public string? Group { get; init; }

    [JsonConstructor]
    public Ingredient(string name, string? amount, string? group)
    {
        Name = name;
        Amount = amount;
        Group = group;
    }
}
