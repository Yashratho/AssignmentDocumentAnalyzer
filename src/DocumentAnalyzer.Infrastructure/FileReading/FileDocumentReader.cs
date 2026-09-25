using DocumentAnalyzer.Application.Interfaces;
using DocumentAnalyzer.Domain.Exceptions;

namespace DocumentAnalyzer.Infrastructure.FileReading;

public class FileDocumentReader : IDocumentReader
{
    public async Task<string> ReadAsync(string filePath, CancellationToken cancellationToken = default)
    {
        if (!File.Exists(filePath))
            throw new DocumentNotFoundException(filePath);

        var content = await File.ReadAllTextAsync(filePath, cancellationToken);

        if (string.IsNullOrWhiteSpace(content))
            throw new DocumentEmptyException(filePath);

        return content;
    }
}
