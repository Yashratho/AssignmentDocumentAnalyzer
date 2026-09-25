namespace DocumentAnalyzer.Application.Interfaces;

public interface IDocumentReader
{
    Task<string> ReadAsync(string filePath, CancellationToken cancellationToken = default);
}
