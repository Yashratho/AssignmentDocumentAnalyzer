using DocumentAnalyzer.Domain.Models;

namespace DocumentAnalyzer.Application.Interfaces;

public interface ILlmService
{
    Task<TechnicalDocument> ExtractDocumentAsync(string documentContent, CancellationToken cancellationToken = default);
    Task<string> AskQuestionAsync(string documentContent, string question, CancellationToken cancellationToken = default);
}
