namespace DocumentAnalyzer.Domain.Exceptions;

public class DocumentEmptyException(string path)
    : Exception($"Document at '{path}' is empty.")
{
    public string Path { get; } = path;
}
