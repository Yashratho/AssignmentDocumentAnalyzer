using DocumentAnalyzer.Domain.Models;

namespace DocumentAnalyzer.Domain.Validation;

public class DocumentValidator
{
    public IReadOnlyList<ValidationError> Validate(TechnicalDocument document)
    {
        var errors = new List<ValidationError>();

        if (string.IsNullOrWhiteSpace(document.ProjectInfo?.ProjectName))
            errors.Add(new ValidationError { Field = "ProjectName", Message = "Project name must not be empty." });

        foreach (var pipe in document.Pipes)
        {
            if (string.IsNullOrWhiteSpace(pipe.Material))
                errors.Add(new ValidationError { Field = $"Pipe[{pipe.Name}].Material", Message = $"Pipe '{pipe.Name}' material must not be empty." });

            if (pipe.DiameterMm <= 0)
                errors.Add(new ValidationError { Field = $"Pipe[{pipe.Name}].DiameterMm", Message = $"Pipe '{pipe.Name}' diameter must be greater than zero." });
        }

        if (document.Insulation != null && document.Insulation.ThicknessMm <= 0)
            errors.Add(new ValidationError { Field = "Insulation.ThicknessMm", Message = "Insulation thickness must be greater than zero." });

        foreach (var equipment in document.Equipment)
        {
            if (string.IsNullOrWhiteSpace(equipment.EquipmentType))
                errors.Add(new ValidationError { Field = "Equipment.EquipmentType", Message = "Equipment type must not be empty." });
        }

        foreach (var req in document.Requirements)
        {
            if (string.IsNullOrWhiteSpace(req.Text) || req.Text.Trim().Length < 5)
                errors.Add(new ValidationError { Field = $"Requirement[{req.Number}]", Message = $"Requirement {req.Number} must contain meaningful text." });
        }

        return errors.AsReadOnly();
    }
}
