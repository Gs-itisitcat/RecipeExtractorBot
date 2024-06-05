using System.Text.Json.Serialization;

namespace RecipeExtractorBot.Serialization;

public record class Recipe
{
    public string? Name { get; init; }
    public string? Serving { get; init; }
    public string? Procedure { get; init; }
    public List<Ingredient> Ingredients { get; init; }

    [JsonConstructor]
    public Recipe(string? name, string? procedure, List<Ingredient> ingredients)
    {
        Name = name;
        Procedure = procedure;
        Ingredients = ingredients;
    }
}
