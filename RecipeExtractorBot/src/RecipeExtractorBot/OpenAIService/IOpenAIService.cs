using RecipeExtractorBot.Serialization;

namespace OpenAIService;

public interface IOpenAIService
{
    /// <summary>
    /// Parses the recipe from the specified description.
    /// </summary>
    /// <param name="description">The description to parse the recipe from.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Returns the parsed recipe.</returns>
    /// <remarks>
    /// The recipe will be null if failed to parse the recipe.
    /// </remarks>
    public Task<IReadOnlyList<Recipe>>  ParseRecipeAsync(string description, CancellationToken cancellationToken = default);
}
