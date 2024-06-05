using System.Text.Json.Serialization;

namespace RecipeExtractorBot.Serialization;

/// <summary>
// Represents a list of recipes that can be parsed from a JSON string.
/// </summary>
internal record class ParsableRecipe
{
    public List<Recipe> Recipes { get; init; }

    [JsonConstructor]
    public ParsableRecipe(List<Recipe> recipes)
    {
        Recipes = recipes;
    }
}
