using System.Text.Json.Serialization;

namespace RecipeExtractorBot.Recipes;

public record class Recipe
{
    public string? Name { get; init; }
    public string? Serving { get; init; }
    public string? Procedure { get; init; }
    public IReadOnlyList<Ingredient> Ingredients { get; init; }

    [JsonConstructor]
    public Recipe(string? name, string? procedure, IReadOnlyList<Ingredient> ingredients)
    {
        Name = name;
        Procedure = procedure;
        Ingredients = ingredients;
    }
}
