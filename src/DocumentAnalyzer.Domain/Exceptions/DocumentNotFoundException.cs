namespace DocumentAnalyzer.Domain.Exceptions;

public class DocumentNotFoundException(string path)
    : Exception($"Document not found at: '{path}'")
{
    public string Path { get; } = path;
}
