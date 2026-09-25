using DocumentAnalyzer.Domain.Models;
using DocumentAnalyzer.Domain.Validation;

namespace DocumentAnalyzer.Application.DTOs;

public class AnalysisResult(TechnicalDocument document, string rawContent, IReadOnlyList<ValidationError> validationErrors)
{
    public TechnicalDocument Document { get; } = document;
    public string RawContent { get; } = rawContent;
    public IReadOnlyList<ValidationError> ValidationErrors { get; } = validationErrors;
    public bool IsValid => ValidationErrors.Count == 0;
}
