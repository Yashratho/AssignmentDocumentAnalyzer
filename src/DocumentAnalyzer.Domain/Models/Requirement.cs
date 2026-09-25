namespace DocumentAnalyzer.Domain.Models;

public class Requirement
{
    public int Number { get; set; }
    public string Text { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
}
