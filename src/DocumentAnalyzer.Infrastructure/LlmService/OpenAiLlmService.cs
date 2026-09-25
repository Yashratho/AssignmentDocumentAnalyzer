using System.ClientModel;
using System.Text.Json;
using System.Text.Json.Serialization;
using DocumentAnalyzer.Application.Interfaces;
using DocumentAnalyzer.Domain.Exceptions;
using DocumentAnalyzer.Domain.Models;
using DocumentAnalyzer.Infrastructure.Options;
using Microsoft.Extensions.Options;
using OpenAI;
using OpenAI.Chat;

namespace DocumentAnalyzer.Infrastructure.LlmService;

public class OpenAiLlmService : ILlmService
{
    private readonly ChatClient _chatClient;
    private readonly AiSettings _options;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public OpenAiLlmService(IOptions<AiSettings> options)
    {
        _options = options.Value;
        ArgumentException.ThrowIfNullOrWhiteSpace(_options.ApiKey);

        var clientOptions = new OpenAIClientOptions();
        if (!string.IsNullOrWhiteSpace(_options.Endpoint))
            clientOptions.Endpoint = new Uri(_options.Endpoint);

        var credential = new ApiKeyCredential(_options.ApiKey);
        _chatClient = new OpenAIClient(credential, clientOptions).GetChatClient(_options.Model);
    }

    public async Task<TechnicalDocument> ExtractDocumentAsync(string documentContent, CancellationToken cancellationToken = default)
    {
        var messages = new List<ChatMessage>
        {
            new SystemChatMessage(PromptBuilder.ExtractionSystemPrompt),
            new UserChatMessage(PromptBuilder.BuildExtractionUserPrompt(documentContent))
        };

        var rawJson = await CallLlmAsync(messages, cancellationToken);
        return Deserialize(rawJson);
    }

    public async Task<string> AskQuestionAsync(string documentContent, string question, CancellationToken cancellationToken = default)
    {
        var messages = new List<ChatMessage>
        {
            new SystemChatMessage(PromptBuilder.QaSystemPrompt),
            new UserChatMessage(PromptBuilder.BuildQaUserPrompt(documentContent, question))
        };

        return await CallLlmAsync(messages, cancellationToken);
    }

    private async Task<string> CallLlmAsync(IEnumerable<ChatMessage> messages, CancellationToken cancellationToken)
    {
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cts.CancelAfter(TimeSpan.FromSeconds(_options.TimeoutSeconds));

        var response = await _chatClient.CompleteChatAsync(messages, cancellationToken: cts.Token);
        var content = response.Value.Content[0].Text;

        if (string.IsNullOrWhiteSpace(content))
            throw new LlmServiceException("LLM returned an empty response.");

        return content.Trim();
    }

    private static TechnicalDocument Deserialize(string rawJson)
    {
        var json = StripCodeBlock(rawJson);

        var document = JsonSerializer.Deserialize<TechnicalDocument>(json, JsonOptions);
        return document ?? throw new LlmServiceException("LLM returned null after deserialization.");
    }

    private static string StripCodeBlock(string raw)
    {
        var text = raw.Trim();

        if (!text.StartsWith("```"))
            return text;

        var firstNewline = text.IndexOf('\n');
        if (firstNewline >= 0)
            text = text[(firstNewline + 1)..];

        if (text.EndsWith("```"))
            text = text[..^3];

        return text.Trim();
    }
}
