using Amazon.Lambda.Annotations;
using Amazon.Lambda.Annotations.APIGateway;
using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;

using RecipeExtractorBot.BotServices;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace RecipeExtractorBot
{
    /// <summary>
    /// Functions that can be invoked by the Lambda service.
    /// </summary>
    public class Functions(IBotService botService)
    {
        private readonly IBotService _botService = botService;

        /// <summary>
        /// Root route that provides information about the other requests that can be made.
        /// </summary>
        /// <returns>API descriptions.</returns>
        [LambdaFunction()]
        [HttpApi(LambdaHttpMethod.Get, "/")]
        public string Default()
        {
            var docs = """
            Recipe Extractor Bot:
            You can use the following routes:
            GET  /                    - This route
            POST /application-command - Handle Discord application commands
            POST /register            - Register the application commands
            """;
            // POST /followup-recipe - Follow up with a recipe URL
            return docs;
        }

        /// <summary>
        /// Handle Discord application commands.
        /// </summary>
        /// <param name="input">The request body from Discord.</param>
        /// <param name="signature">The signature to verify the request from Discord.</param>
        /// <param name="timestamp">The timestamp to verify the request from Discord.</param>
        /// <param name="context">The Lambda context.</param>
        /// <returns>The response to send to Discord.</returns>
        [LambdaFunction(Policies = "AWSLambdaBasicExecutionRole")]
        [HttpApi(LambdaHttpMethod.Post, "/application-command")]
        public IHttpResult ApplicationCommand([FromBody] string input,
        [FromHeader(Name = "X-Signature-Ed25519")] string? signature,
        [FromHeader(Name = "X-Signature-Timestamp")] string? timestamp,
        ILambdaContext context)
        {
            var applicationCommandTask = _botService.ApplicationCommandAsync(input, signature, timestamp, context);

            // Discord fails to receive the response when it is returned as a Task, so wait for the Task to complete.
            while (!applicationCommandTask.IsCompleted)
            {
                Task.Delay(100).Wait();
            }
            return applicationCommandTask.Result;

        }

        /// <summary>
        /// Follow up with a recipe URL.
        /// </summary>
        /// <param name="input">The URL and token to follow up with.</param>
        /// <param name="context">The Lambda context.</param>
        /// <returns>The response to send to Discord.</returns>
        /// <remarks>
        /// This function will not be called from the API Gateway.
        /// </remarks>

        [LambdaFunction()]
        // [HttpApi(LambdaHttpMethod.Post, "/followup-recipe")]
        public async ValueTask<APIGatewayProxyResponse> FollowupRecipe(object input, ILambdaContext context)
        {
            var response = await _botService.FollowupRecipeAsync(input.ToString() ?? string.Empty, context);
            context.Logger.LogInformation($"FollowupRecipe response: {response.StatusCode}");
            var body = await response.Content.ReadAsStringAsync();
            context.Logger.LogDebug($"FollowupRecipe response body: {body}");
            return new APIGatewayProxyResponse
            {
                StatusCode = (int)response.StatusCode,
                Body = body,
                Headers = new Dictionary<string, string>
                {
                    { "Content-Type", "application/json" }
                }
            };
        }

        [LambdaFunction()]
        [HttpApi(LambdaHttpMethod.Post, "/register")]
        public IHttpResult RegisterApplicationCommands([FromBody] string input, ILambdaContext context)
        {
            context.Logger.LogInformation("Registering application commands");
            context.Logger.LogInformation($"Input: {input}");
            return _botService.RegisterCommands(context);
        }

    }
}
