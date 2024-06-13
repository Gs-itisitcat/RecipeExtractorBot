using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Amazon.Lambda;
using Amazon.Lambda.Core;
using Amazon.Lambda.Annotations.APIGateway;
using RecipeExtractorBot.ResponseServices;
using RecipeExtractorBot.RecipeExtractor;
using RecipeExtractorBot.Recipes;
using RecipeExtractorBot.DiscordInteractions;
using RecipeExtractorBot.VideoInformationServices;

namespace RecipeExtractorBot.BotServices;

public class DiscordBotService(IRecipeExtractor recipeExtractor, IResponseService responseService, IAmazonLambda amazonLambda, IConfiguration configuration) : IBotService
{
    private readonly IRecipeExtractor _recipeExtractor = recipeExtractor;
    private readonly IResponseService _responseService = responseService;
    private readonly IAmazonLambda _lambdaClient = amazonLambda;

    private readonly string _discordBotPublicKey = configuration.GetValue<string>("DISCORD_BOT_PUBLIC_KEY") ?? string.Empty;
    private readonly string _discordBotApplicationId = configuration.GetValue<string>("DISCORD_BOT_APPLICATION_ID") ?? string.Empty;
    private readonly string _discordBotToken = configuration.GetValue<string>("DISCORD_BOT_TOKEN") ?? string.Empty;
    private readonly string _recipeLambdaFunctionName = configuration.GetValue<string>("RECIPE_LAMBDA_FUNCTION_NAME") ?? string.Empty;

    private readonly int _maxRetries = configuration.GetValue<int>("MAX_RETRIES", 3);
    private readonly TimeSpan _retryDelay = TimeSpan.FromSeconds(configuration.GetValue<float>("RETRY_DELAY", 1));
    private readonly TimeSpan _timeout = TimeSpan.FromSeconds(configuration.GetValue<float>("TIMEOUT", 30));
    private static readonly JsonSerializerOptions _discordJsonSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
    };

    public async ValueTask<IHttpResult> ApplicationCommandAsync(string input, string? signature, string? timestamp, ILambdaContext context, CancellationToken cancellationToken = default)
    {
        var isValid = _discordBotPublicKey is not null && Verification.VerifySignature(input, signature, timestamp, _discordBotPublicKey);
        if (!isValid)
        {
            context.Logger.LogError("Verification failed. Invalid signature or public key.");
            return HttpResults.BadRequest("Verification failed. Invalid signature or public key.");
        }

        context.Logger.LogInformation("Verification successful.");

        var interaction = JsonSerializer.Deserialize<Interaction>(input, _discordJsonSerializerOptions);
        if (interaction is null)
        {
            context.Logger.LogError("Invalid interaction object");
            return HttpResults.BadRequest("Invalid interaction object");
        }

        var applicationId = interaction.ApplicationId;
        if (applicationId != _discordBotApplicationId)
        {
            context.Logger.LogError("Invalid application ID");
            return HttpResults.BadRequest("Invalid application ID");
        }

        context.Logger.LogInformation(input);

        var interactionType = interaction.Type;
        context.Logger.LogInformation($"Received interaction of type {interactionType}");

        var applicationCommandType = interaction.Data?.Type;

        return (interactionType, applicationCommandType) switch
        {
            (InteractionType.PING, _) => _responseService.Pong(),
            (InteractionType.APPLICATION_COMMAND, ApplicationCommandType.CHAT_INPUT) => await SlashCommandAsync(interaction, context, cancellationToken),
            (InteractionType.APPLICATION_COMMAND, _) => HttpResults.BadRequest("Invalid Application Command type."),
            _ => HttpResults.BadRequest("Invalid interaction type.")
        };
    }

    /// <summary>
    /// Handles the slash command of the application command.
    /// </summary>
    /// <param name="interaction">Interaction object from Discord.</param>
    /// <param name="context">Lambda context.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The response to send to Discord.</returns>
    private async ValueTask<IHttpResult> SlashCommandAsync(Interaction interaction, ILambdaContext context, CancellationToken cancellationToken = default)
    {
        var command = interaction.Data?.Name;
        var options = interaction.Data?.Options;
        if (command is null)
        {
            context.Logger.LogError("No command provided.");
            return _responseService.Error("No command provided.");
        }

        context.Logger.LogInformation($"Received command {command}");

        return command switch
        {
            _ when !ApplicationCommand.ApplicationCommands.Where(c => c.Type == ApplicationCommandType.CHAT_INPUT).Any(c => c.Name == command) => HttpResults.BadRequest($"Command {command} is not implemented."),
            "recipe-help" => _responseService.HelpMessage(),
            "recipe" => await RecipeCommandAsync(options, interaction.Token, context, cancellationToken),
            _ => HttpResults.NewResult(System.Net.HttpStatusCode.NotImplemented, $"Command {command} is registered but not implemented."),
        };
    }

    /// <summary>
    /// Handles the recipe command of the slash command.
    /// </summary>
    /// <param name="options">Options provided with the command.</param>
    /// <param name="token">Interaction token.</param>
    /// <param name="context">Lambda context.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The response to send to Discord.</returns>
    /// <remarks>
    /// This function will invoke the recipe lambda function to extract recipes from the URL, because the recipe extraction process is time-consuming.
    /// </remarks>
    private async ValueTask<IHttpResult> RecipeCommandAsync(IReadOnlyList<ApplicationCommandOption>? options, string token, ILambdaContext context, CancellationToken cancellationToken = default)
    {
        if (options is null)
        {
            context.Logger.LogError("No options provided.");
            return _responseService.Error("No options provided.");
        }

        var urlOption = options.FirstOrDefault(o => o.Name == "url");
        if (urlOption is null)
        {
            context.Logger.LogError("No URL option provided.");
            return _responseService.Error("No URL option provided.");
        }

        var url = urlOption.Value;
        if (url is null)
        {
            context.Logger.LogError("No URL provided.");
            return _responseService.Error("No URL provided.");
        }

        if (!_recipeExtractor.IsSupported(url))
        {
            context.Logger.LogError($"The provided text is not a supported URL: {url}");
            return _responseService.Error($"The provided text is not a supported URL: {url}");
        }

        context.Logger.LogDebug($"Received URL {url}");


        var request = new Amazon.Lambda.Model.InvokeRequest()
        {
            FunctionName = _recipeLambdaFunctionName,
            InvocationType = InvocationType.Event,
            Payload = JsonSerializer.Serialize(new FollowupContent(url, token), _discordJsonSerializerOptions)
        };

        if (_lambdaClient is null)
        {
            context.Logger.LogError("Lambda client is null");
            return HttpResults.InternalServerError("Lambda client is null");
        }

        context.Logger.LogInformation("Invoking recipe lambda function");
        await _lambdaClient.InvokeAsync(request, cancellationToken);

        return _responseService.PostURL(url);
    }

    public async ValueTask<HttpResponseMessage> FollowupRecipeAsync(string input, ILambdaContext context, CancellationToken cancellationToken = default)
    {
        var followUpContent = JsonSerializer.Deserialize<FollowupContent>(input, _discordJsonSerializerOptions);
        var url = followUpContent?.URL;
        var token = followUpContent?.Token;

        if (url is null)
        {
            context.Logger.LogError("No URL provided.");
            return new HttpResponseMessage()
            {
                StatusCode = System.Net.HttpStatusCode.BadRequest,
                Content = new StringContent("No URL provided.")
            };
        }

        if (token is null)
        {
            context.Logger.LogError("No token provided.");
            return new HttpResponseMessage()
            {
                StatusCode = System.Net.HttpStatusCode.BadRequest,
                Content = new StringContent("No token provided.")
            };
        }

        context.Logger.LogDebug($"Received URL {url}");

        var loadingMessageTask = _responseService.FollowupLoadingAsync(_discordBotApplicationId, token, cancellationToken);
        context.Logger.LogInformation("Sending loading message.");


        context.Logger.LogInformation("Extracting recipe.");
        int retries = 0;
        IVideoInformation? information = null;
        IReadOnlyList<Recipe> recipes = [];
        while (retries < _maxRetries)
        {
            using var timeoutCancellationTokenSource = new CancellationTokenSource(_timeout);
            using var linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCancellationTokenSource.Token);
            try
            {
                (information, recipes) = await _recipeExtractor.ExtractRecipeAsync(url, linkedTokenSource.Token);
                // DO NOT remove this `break` statement. It is required to break out of the retry loop when the extraction is successful.
                break;
            }
            catch (Exception ex) when (ex is JsonException or OperationCanceledException)
            {
                switch (ex)
                {
                    case JsonException:
                        context.Logger.LogError("Failed to parse JSON while extracting recipe.");
                        break;
                    case OperationCanceledException when timeoutCancellationTokenSource.IsCancellationRequested:
                        context.Logger.LogError("Extraction operation was canceled due to timeout.");
                        break;
                    case OperationCanceledException:
                        // Unknown cancellation exception. Log the error and rethrow.
                        context.Logger.LogError("Extraction operation was canceled.");
                        throw;
                    default:
                        throw;
                }
            }

            if (retries >= _maxRetries)
            {
                context.Logger.LogError($"Extraction failed after maximum retries ({_maxRetries}).");
                return await _responseService.FollowupErrorAsync("Extraction failed. Please try again later.", _discordBotApplicationId, token, cancellationToken);
            }

            await Task.Delay(_retryDelay, cancellationToken);
            retries++;
            context.Logger.LogInformation($"Retrying extraction. Retry count: {retries}/{_maxRetries}");
        }
        context.Logger.LogInformation("Recipe extracted.");

        // Make sure to await the loading message response before sending the followup response.
        using var loadingMessageResponse = await loadingMessageTask;
        context.Logger.LogInformation($"Loading message status code: {loadingMessageResponse.StatusCode}");

        if (information is null)
        {
            context.Logger.LogError("Failed to get video information. The URL may not be a valid video URL.");
            return await _responseService.FollowupErrorAsync("Failed to get video information. The URL may not be a valid video URL.", _discordBotApplicationId, token, cancellationToken);
        }

        if (recipes.Count == 0)
        {
            context.Logger.LogWarning("No recipes found.");
            return await _responseService.FollowupErrorAsync("No recipes found.", _discordBotApplicationId, token, cancellationToken);
        }

        context.Logger.LogInformation($"Extracted {recipes.Count} recipes.");
        context.Logger.LogDebug($"Video title: {information.Title}");
        context.Logger.LogDebug($"Video URL: {information.URL}");
        context.Logger.LogDebug($"Recipes: {string.Join(", ", recipes.Select(r => r.ToString()))}");

        var embeds = recipes.Select((recipe, recipeIndex) =>
        {
            context.Logger.LogDebug($"Ingredients: {string.Join(", ", recipe.Ingredients.Select(i => i.ToString()))}");
            var groupedIngredients = recipe.Ingredients
            .ToLookup(i => i.Group);

            var descriptionBuilder = new StringBuilder();

            if (recipe.Name is not null and not "null")
            {
                descriptionBuilder.AppendLine($"**{recipe.Name}**");
            }

            if (recipe.Serving is not null and not "null")
            {
                descriptionBuilder.AppendLine($"**分量:** {recipe.Serving}");
            }

            if (recipe.Procedure is not null and not "null")
            {
                descriptionBuilder.AppendLine();
                descriptionBuilder.AppendLine(recipe.Procedure);
            }

            // Discord embeds have a limit of 25 fields per embed.
            // So, if the number of ingredients exceeds 25, add them to the description instead of the fields.
            List<Field>? fields = null;
            var expectedFields = recipe.Ingredients.Count + groupedIngredients.Count;
            if (expectedFields > 25)
            {
                context.Logger.LogInformation("Adding ingredients to the description.");
                foreach (var group in groupedIngredients)
                {
                    context.Logger.LogDebug($"Group: {group.Key}");

                    // Spaces between groups are not enough to separate them with just a newline.
                    descriptionBuilder.AppendLine().AppendLine();
                    if (group.Key is not null and not "null")
                    {
                        descriptionBuilder.AppendLine($"**{group.Key}**");
                    }
                    descriptionBuilder.AppendLine("--------------------");

                    descriptionBuilder.AppendJoin("\n", group.Select(i => $"**{i.Name}:** {(i.Amount is null or "null" ? string.Empty : i.Amount)}"));
                }
            }
            else
            {
                context.Logger.LogInformation("Adding ingredients to the embed fields.");

                fields = [];
                foreach (var group in groupedIngredients)
                {
                    context.Logger.LogInformation($"Group: {group.Key}");
                    fields.Add(new Field(group.Key is null or "null" ? string.Empty : group.Key, "--------------------", false));

                    fields.AddRange(group.Select(i => new Field(i.Name, i.Amount is null or "null" ? string.Empty : i.Amount, true)));
                    fields[^1].Inline = false;
                }

                fields[^1].Inline = true;
            }

            return new Embed()
            {
                Title = $"{information.Title}{(recipes.Count > 1 ? $" ( {recipeIndex + 1}/{recipes.Count})" : string.Empty)}",
                Description = descriptionBuilder.ToString(),
                Url = information.URL,
                Author = information.ChannelName is null ? null : new Author(information.ChannelName)
                {
                    Url = information.ChannelURL,
                    IconUrl = information.ChannelIcon,
                },
                Color = 15409955,
                Image = new EmbedImage()
                {
                    Url = information.Thumbnail,
                },
                Fields = fields,
            };
        });

        // By some reason, sending multiple embeds in a single followup response is not working.
        // It results in only the first embed being sent.
        // So, send each embed in a separate followup response.
        var followupResponses = embeds.Select(embed => new FollowUpResponse()
        {
            Content = string.Empty,
            Embeds = [embed],
        });

        var followupResponseTasks = followupResponses.Select(async followupResponse => await _responseService.FollowupRecipeAsync(followupResponse, _discordBotApplicationId, token, cancellationToken));
        var responses = await Task.WhenAll(followupResponseTasks);


        var failedResponses = responses.Where(response => response.StatusCode != System.Net.HttpStatusCode.OK);
        if (failedResponses.Any())
        {
            context.Logger.LogError("Failed to send one or more recipes.");
            var followupErrorTask = _responseService.FollowupErrorAsync("One or more recipes failed to send. Please try again later.", _discordBotApplicationId, token, cancellationToken);

            foreach (var failedResponse in failedResponses)
            {
                context.Logger.LogError($"Failed to send recipe. Status code: {failedResponse.StatusCode}. Body: {await failedResponse.Content.ReadAsStringAsync(cancellationToken)}");
            }

            var followupErrorResponse = await followupErrorTask;
            context.Logger.LogInformation($"Followup error response status code: {followupErrorResponse.StatusCode}");
            context.Logger.LogDebug($"Followup error response body: {await followupErrorResponse.Content.ReadAsStringAsync(cancellationToken)}");

            return new HttpResponseMessage()
            {
                StatusCode = System.Net.HttpStatusCode.InternalServerError,
                Content = new StringContent($"{failedResponses.Count()} of {responses.Length} recipes failed to send. Please try again later."),
            };
        }

        context.Logger.LogInformation($"{responses.Length} followup responses sent.");

        foreach (var response in responses)
        {
            context.Logger.LogInformation($"Followup response status code: {response.StatusCode}");
            context.Logger.LogDebug($"Followup response body: {await response.Content.ReadAsStringAsync(cancellationToken)}");
        }

        return new HttpResponseMessage()
        {
            StatusCode = System.Net.HttpStatusCode.OK,
            Content = new StringContent($"Followup responses sent: {responses.Length}."),
        };
    }

    public IHttpResult RegisterCommands(ILambdaContext context, CancellationToken cancellationToken = default)
    {
        var commands = ApplicationCommand.ApplicationCommands;
        context.Logger.LogInformation($"Registering {commands.Count} commands.");
        var response = _responseService.RegisterCommands(commands, _discordBotApplicationId, _discordBotToken, cancellationToken);
        context.Logger.LogInformation($"Register commands response: {response.StatusCode}");
        return response;
    }
}
