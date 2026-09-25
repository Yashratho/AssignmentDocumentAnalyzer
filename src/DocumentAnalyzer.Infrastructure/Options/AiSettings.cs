namespace DocumentAnalyzer.Infrastructure.Options;

public class AiSettings
{
    public const string SectionName = "AiSettings";

    public string Provider { get; set; } = "GoogleGemini";
    public string Endpoint { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public string Model { get; set; } = "gemini-3.1-flash-lite";
    public int TimeoutSeconds { get; set; } = 60;
}
