namespace DocumentAnalyzer.Domain.Models;

public class TechnicalDocument
{
    public ProjectInfo ProjectInfo { get; set; } = new();
    public List<PipeSpecification> Pipes { get; set; } = [];
    public List<EquipmentSpecification> Equipment { get; set; } = [];
    public InsulationSpecification? Insulation { get; set; }
    public List<Requirement> Requirements { get; set; } = [];
}
