namespace DocumentAnalyzer.Domain.Exceptions;

public class LlmServiceException(string message, Exception? inner = null)
    : Exception(message, inner);
