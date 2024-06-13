using System.Text.Json;
using System.Net.Http.Json;
using Amazon.Lambda.Annotations.APIGateway;
using RecipeExtractorBot.DiscordInteractions;

namespace RecipeExtractorBot.ResponseServices;

public class ResponseService : IResponseService
{
    private static readonly string _webhookEndpoint = "https://discord.com/api/v10";
    private static readonly JsonSerializerOptions _serializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
    };

    public IHttpResult Pong()
    {
        return HttpResults.Ok(JsonSerializer.Serialize(new InteractionResponse()
        {
            Type = InteractionCallbackType.PONG,
            Data = new Message()
            {
                Content = "pong",
                Flags = MessageFlags.EPHEMERAL,
            }
        }, _serializerOptions))
        .AddHeader("Content-Type", "application/json");
    }

    public IHttpResult HelpMessage()
    {
        var commands = ApplicationCommand.ApplicationCommands.Select(command => $"`/{command.Name}` - {command.Description}")
        .Aggregate((current, next) => $"{current}\n{next}");
        return HttpResults.Ok(JsonSerializer.Serialize(new InteractionResponse()
        {
            Type = InteractionCallbackType.CHANNEL_MESSAGE_WITH_SOURCE,
            Data = new Message()
            {
                Content = $"""
                    **Recipe Extractor Bot**
                    Extract recipes from URLs.

                    **Commands**
                    {commands}

                    **Available URLs**
                    - YouTube: `https://www.youtube.com/watch?v=video_id`

                    **Example**
                    `/recipe https://www.youtube.com/watch?v=video_id`

                    **Note**
                    - The bot uses ChatGPT for recipe extraction.
                    - The bot is not responsible for the content extracted from URLs.
                    """,
                Flags = MessageFlags.EPHEMERAL,
            }
        }, _serializerOptions))
        .AddHeader("Content-Type", "application/json");
    }

    public IHttpResult PostURL(string url)
    {
        return HttpResults.Ok(JsonSerializer.Serialize(new InteractionResponse()
        {
            Type = InteractionCallbackType.CHANNEL_MESSAGE_WITH_SOURCE,
            Data = new Message()
            {
                Content = url,
            }
        }, _serializerOptions))
        .AddHeader("Content-Type", "application/json");
    }

    public async ValueTask<HttpResponseMessage> FollowupRecipeAsync(FollowUpResponse followUpResponse, string applicationId, string token, CancellationToken cancellationToken = default)
    {
        var url = $"{_webhookEndpoint}/webhooks/{applicationId}/{token}";
        var client = new HttpClient();
        var content = JsonContent.Create(followUpResponse, options: _serializerOptions);
        var response = await client.PostAsync(url, content, cancellationToken);
        return response;
    }

    public async ValueTask<HttpResponseMessage> FollowupRecipeAsync(FollowUpResponse followUpResponse, string applicationId, string token, string messageId, CancellationToken cancellationToken = default)
    {
        var url = $"{_webhookEndpoint}/webhooks/{applicationId}/{token}/messages/{messageId}";
        var client = new HttpClient();
        var content = JsonContent.Create(followUpResponse, options: _serializerOptions);
        var response = await client.PatchAsync(url, content, cancellationToken);
        return response;
    }

    public async ValueTask<HttpResponseMessage> FollowupLoadingAsync(string applicationId, string token, CancellationToken cancellationToken = default)
    {
        var url = $"{_webhookEndpoint}/webhooks/{applicationId}/{token}";
        var client = new HttpClient();
        var content = JsonContent.Create(new FollowUpResponse()
        {
            Content = "Extracting recipe...",
            // Loading flag is not supported for follow-up messages
            // Flags = MessageFlags.LOADING,
            Flags = MessageFlags.EPHEMERAL,
        }, options: _serializerOptions);
        var response = await client.PostAsync(url, content, cancellationToken);
        return response;
    }

    public IHttpResult Error(string message)
    {
        return HttpResults.Ok(JsonSerializer.Serialize(new InteractionResponse()
        {
            Type = InteractionCallbackType.CHANNEL_MESSAGE_WITH_SOURCE,
            Data = new Message()
            {
                Content = message,
                Flags = MessageFlags.EPHEMERAL,
            }
        }, _serializerOptions))
        .AddHeader("Content-Type", "application/json");
    }

    public async ValueTask<HttpResponseMessage> FollowupErrorAsync(string message, string applicationId, string token, CancellationToken cancellationToken = default)
    {
        var url = $"{_webhookEndpoint}/webhooks/{applicationId}/{token}";
        var client = new HttpClient();
        var content = JsonContent.Create(new FollowUpResponse()
        {
            Content = message,
            Flags = MessageFlags.EPHEMERAL,
        }, options: _serializerOptions);
        var response = await client.PostAsync(url, content, cancellationToken);
        return response;
    }

    public async ValueTask<HttpResponseMessage> FollowupErrorAsync(string message, string applicationId, string token, string messageId, CancellationToken cancellationToken = default)
    {
        var url = $"{_webhookEndpoint}/webhooks/{applicationId}/{token}/messages/{messageId}";
        var client = new HttpClient();
        var content = JsonContent.Create(new FollowUpResponse()
        {
            Content = message,
            Flags = MessageFlags.EPHEMERAL,
        }, options: _serializerOptions);
        var response = await client.PatchAsync(url, content, cancellationToken);
        return response;
    }

    public IHttpResult RegisterCommands(IReadOnlyList<ApplicationCommand> applicationCommands, string applicationId, string accessToken, CancellationToken cancellationToken = default)
    {
        var url = $"{_webhookEndpoint}/applications/{applicationId}/commands";
        using var client = new HttpClient();
        client.DefaultRequestHeaders.Authorization = new("Bot", accessToken);
        foreach (var command in applicationCommands)
        {
            using var content = JsonContent.Create(command, options: _serializerOptions);
            // Somehow the application terminates when awaiting the Post, so do not await
            _ = client.PostAsJsonAsync(url, command, options: _serializerOptions, cancellationToken: cancellationToken);
        }

        return HttpResults.Ok($"Successfully registered {applicationCommands.Count} commands.")
        .AddHeader("Content-Type", "text/plain");
    }
}
