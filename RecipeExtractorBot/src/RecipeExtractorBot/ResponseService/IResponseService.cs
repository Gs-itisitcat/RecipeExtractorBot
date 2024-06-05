using Amazon.Lambda.Annotations.APIGateway;
using RecipeExtractorBot.Serialization.DiscordInteraction.ApplicationCommands;
using RecipeExtractorBot.Serialization.DiscordInteraction.FollowUp;

namespace RecipeExtractorBot.Response;
public interface IResponseService
{
    /// <summary>
    /// Pong response for health checks.
    /// </summary>
    /// <returns>Returns a pong response</returns>
    public IHttpResult Pong();

    /// <summary>
    /// Help message for the bot.
    /// </summary>
    /// <returns>Returns a help message</returns>
    /// <remarks>
    /// This method is called when a user interacts with the "recipe-help" slash command.
    public IHttpResult HelpMessage();

    /// <summary>
    /// Post the URL to the Discord channel.
    /// </summary>
    /// <param name="url">The URL to post.</param>
    /// <returns>Returns the URL to the Discord channel</returns>
    /// <remarks>
    /// This method is called when a user interacts with the "recipe" slash command.
    /// </remarks>
    public IHttpResult PostURL(string url);

    /// <summary>
    /// Follow up with a recipes extracted from a URL.
    /// </summary>
    /// <param name="followUpResponse">Recipes response to follow up with.</param>
    /// <param name="applicationId">The application ID to follow up with.</param>
    /// <param name="token">The original interaction token to follow up with.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Returns the response to send to Discord.</returns>
    /// <remarks>
    /// This method is called from the bot service to follow up with recipes extracted from a URL.
    /// </remarks>
    public ValueTask<HttpResponseMessage> FollowupRecipeAsync(FollowUpResponse followUpResponse, string applicationId, string token, CancellationToken cancellationToken = default);

    /// <summary>
    /// Follow up with a recipes extracted from a URL.
    /// </summary>
    /// <param name="followUpResponse">Recipes response to follow up with.</param>
    /// <param name="applicationId">The application ID to follow up with.</param>
    /// <param name="token">The original interaction token to follow up with.</param>
    /// <param name="messageId">The loading message ID to overwrite.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Returns the response to send to Discord.</returns>
    /// <remarks>
    /// This method is called from the bot service to overwrite a loading message with recipes extracted from a URL.
    /// </remarks>
    public ValueTask<HttpResponseMessage> FollowupRecipeAsync(FollowUpResponse followUpResponse, string applicationId, string token, string messageId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Follow up with a loading message.
    /// </summary>
    /// <param name="applicationId">The application ID to follow up with.</param>
    /// <param name="token">The original interaction token to follow up with.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Returns the response to send to Discord.</returns>
    /// <remarks>
    /// This method is called from the bot service to follow up with a loading message.
    /// </remarks>
    public ValueTask<HttpResponseMessage> FollowupLoadingAsync(string applicationId, string token, CancellationToken cancellationToken = default);

    /// <summary>
    /// Post an error message to the Discord channel.
    /// </summary>
    /// <param name="message">The error message to post.</param>
    /// <returns>Returns the error message to the Discord channel</returns>
    /// <remarks>
    /// This method is called when an error to notify the user. The message is ephemeral.
    /// </remarks>
    public IHttpResult Error(string message);

    /// <summary>
    /// Follow up with an error message.
    /// </summary>
    /// <param name="message">The error message to follow up with.</param>
    /// <param name="applicationId">The application ID to follow up with.</param>
    /// <param name="token">The original interaction token to follow up with.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Returns the response to send to Discord.</returns>
    /// <remarks>
    /// This method is called when an error to notify the user in a follow-up message. The message is ephemeral.
    /// </remarks>
    public ValueTask<HttpResponseMessage> FollowupErrorAsync(string message, string applicationId, string token, CancellationToken cancellationToken = default);

    /// <summary>
    /// Follow up with an error message.
    /// </summary>
    /// <param name="message">The error message to follow up with.</param>
    /// <param name="applicationId">The application ID to follow up with.</param>
    /// <param name="token">The original interaction token to follow up with.</param>
    /// <param name="messageId">The loading message ID to overwrite.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Returns the response to send to Discord.</returns>
    /// <remarks>
    /// This method is called when an error to notify the user in a follow-up message. The message is ephemeral.
    /// </remarks>
    public ValueTask<HttpResponseMessage> FollowupErrorAsync(string message, string applicationId, string token, string messageId, CancellationToken cancellationToken = default);

    public IHttpResult RegisterCommands(IReadOnlyList<ApplicationCommand> applicationCommands, string applicationId, string accessToken, CancellationToken cancellationToken = default);
}
