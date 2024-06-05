using Amazon.Lambda.Core;
using Amazon.Lambda.Annotations.APIGateway;

namespace RecipeExtractorBot.BotService;

public interface IBotService
{
    /// <summary>
    /// Handles the application command from Discord.
    /// </summary>
    /// <param name="input">Request body from Discord.</param>
    /// <param name="signature">Signature to verify the request from Discord.</param>
    /// <param name="timestamp">Timestamp to verify the request from Discord.</param>
    /// <param name="context">Lambda context.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The response to send to Discord.</returns>
    /// <remarks>
    /// This method is called when a user interacts with the Discord bot.
    /// </remarks>
    public ValueTask<IHttpResult> ApplicationCommandAsync(string input, string? signature, string? timestamp, ILambdaContext context, CancellationToken cancellationToken = default);
    /// <summary>
    /// Follow up with recipes extracted from a URL.
    /// </summary>
    /// <param name="input">URL and token to follow up with.</param>
    /// <param name="context">Lambda context.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The response to send to Discord.</returns>
    public ValueTask<HttpResponseMessage> FollowupRecipeAsync(string input, ILambdaContext context, CancellationToken cancellationToken = default);
    public IHttpResult RegisterCommands(ILambdaContext context, CancellationToken cancellationToken = default);
}
