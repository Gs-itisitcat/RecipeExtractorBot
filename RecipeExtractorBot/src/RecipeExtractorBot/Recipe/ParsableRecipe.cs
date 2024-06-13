using System.Text.Json.Serialization;

namespace RecipeExtractorBot.Serialization;

/// <summary>
// Represents a list of recipes that can be parsed from a JSON string.
/// </summary>
internal record class ParsableRecipe
{
    public IReadOnlyList<Recipe> Recipes { get; init; }

    [JsonConstructor]
    public ParsableRecipe(IReadOnlyList<Recipe> recipes)
    {
        Recipes = recipes;
    }
}
