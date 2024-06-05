using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Azure.AI.OpenAI;
using RecipeExtractorBot.Serialization;

namespace OpenAIService;

public class ChatGPTService(IConfiguration configuration) : IOpenAIService
{
    private readonly string _modelId = configuration.GetValue<string>("OPENAI_MODEL_ID", "gpt-4o") ?? "gpt-4o";
    private readonly OpenAIClient _openAIClient = new(configuration.GetValue<string>("OPENAI_API_KEY") ?? string.Empty, new());
    private static readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        AllowTrailingCommas = true,
        PropertyNameCaseInsensitive = true,
    };

    private static readonly string _systemPrompt = """
    あなたはデータアナリストだ。
    あるYouTube動画の動画説明欄の内容を与える。
    この中から料理名、料理手順、材料一覧を抜き出し、以下のフォーマットで返しなさい
    食材がグループ分けされている場合、そのグループ名を示すこと。
    また何人前かが明記されている場合、その情報も含めること。
    料理手順は改行なども含めて原文ママとすること。
    該当する情報が無い場合nullとすること。
    動画説明欄の内容に情報が含まれない場合`recipes`を空の配列として返すこと。
    ```json
    {
        "recipes": [
            {
                "name": "<料理名>"
                "serving": "<何人前か>"
                "procedure": "<料理の手順>"
                "ingredients": [
                    {
                        "name": "<食材名>",
                        "amount": "<食材の量>",
                        "group": "<食材クループ名>"
                    }
                ]
            }
        ]
    }
    ```
    """;

    /// <summary>
    /// Parse the recipe from the description.
    /// </summary>
    /// <param name="description">The description to parse the recipe from.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Returns the parsed recipe.</returns>
    /// <exception cref="JsonException">Failed to parse the JSON from ChatGPT response.</exception>
    /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
    public async Task<IReadOnlyList<Recipe>> ParseRecipeAsync(string description, CancellationToken cancellationToken = default)
    {
        var chatCompletionsOptions = new ChatCompletionsOptions()
        {
            DeploymentName = _modelId,
            Messages =
            {
                new ChatRequestSystemMessage(_systemPrompt),
                new ChatRequestUserMessage(description),
            },
            Temperature = 0,
        };

        var response = await _openAIClient.GetChatCompletionsAsync(chatCompletionsOptions, cancellationToken);
        var reply = response.Value.Choices[0].Message.Content;
        var recipesJson = reply.TrimStart("```json\n".ToCharArray()).TrimEnd("\n```".ToCharArray());
        var recipesDict = JsonSerializer.Deserialize<ParsableRecipe>(recipesJson, _jsonSerializerOptions);
        var recipes = recipesDict?.Recipes;

        return recipes ?? [];
    }
}
