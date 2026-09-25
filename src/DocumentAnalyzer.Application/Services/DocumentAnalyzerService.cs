using DocumentAnalyzer.Application.DTOs;
using DocumentAnalyzer.Application.Interfaces;
using DocumentAnalyzer.Domain.Validation;

namespace DocumentAnalyzer.Application.Services;

public class DocumentAnalyzerService(IDocumentReader documentReader, ILlmService llmService, DocumentValidator validator)
{
    public async Task<AnalysisResult> AnalyzeAsync(string filePath, CancellationToken cancellationToken = default)
    {
        var rawContent = await documentReader.ReadAsync(filePath, cancellationToken);
        var document = await llmService.ExtractDocumentAsync(rawContent, cancellationToken);
        var errors = validator.Validate(document);
        return new AnalysisResult(document, rawContent, errors);
    }

    public async Task<string> AskQuestionAsync(string rawContent, string question, CancellationToken cancellationToken = default)
    {
        return await llmService.AskQuestionAsync(rawContent, question, cancellationToken);
    }
}
