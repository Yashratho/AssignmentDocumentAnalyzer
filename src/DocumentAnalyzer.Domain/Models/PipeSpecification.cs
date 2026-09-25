namespace DocumentAnalyzer.Domain.Models;

public class PipeSpecification
{
    public string Name { get; set; } = string.Empty;
    public string Material { get; set; } = string.Empty;
    public double DiameterMm { get; set; }
    public string PressureRating { get; set; } = string.Empty;
    public double LengthM { get; set; }
}
