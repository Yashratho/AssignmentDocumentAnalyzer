using DocumentAnalyzer.Application.DTOs;
using DocumentAnalyzer.Application.Services;
using DocumentAnalyzer.Domain.Exceptions;
using DocumentAnalyzer.Domain.Models;
using DocumentAnalyzer.Domain.Validation;

namespace DocumentAnalyzer.ConsoleApp;

public class ConsoleRunner(DocumentAnalyzerService analyzerService)
{
    public async Task RunAsync(string documentPath, CancellationToken cancellationToken = default)
    {
        Console.WriteLine("========================================");
        Console.WriteLine("  Technical Document Analyzer");
        Console.WriteLine("========================================");
        Console.WriteLine();

        AnalysisResult result;
        try
        {
            Console.WriteLine($"Reading: {documentPath}");
            Console.WriteLine("Analyzing document with LLM, please wait...");
            result = await analyzerService.AnalyzeAsync(documentPath, cancellationToken);
            Console.WriteLine("Done.\n");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n[ERROR] Analysis failed: {ex.Message}");
            Console.WriteLine("If you see '404', the LLM model name might be incorrect in appsettings.json. If '429', the API is overloaded.");
            return;
        }

        ShowDocument(result.Document);
        ShowValidation(result.ValidationErrors);
        ShowRequirementCheck(result.Document);
        await RunQaLoop(result.RawContent, cancellationToken);
    }

    private static void ShowDocument(TechnicalDocument doc)
    {
        Console.WriteLine("--- PROJECT INFORMATION ---");
        Console.WriteLine($"  Name       : {doc.ProjectInfo.ProjectName}");
        Console.WriteLine($"  Discipline : {doc.ProjectInfo.Discipline}");
        Console.WriteLine($"  System     : {doc.ProjectInfo.System}");

        Console.WriteLine($"\n--- PIPE SPECIFICATIONS ({doc.Pipes.Count}) ---");
        foreach (var pipe in doc.Pipes)
        {
            Console.WriteLine($"  {pipe.Name}");
            Console.WriteLine($"    Material       : {pipe.Material}");
            Console.WriteLine($"    Diameter       : {pipe.DiameterMm} mm");
            Console.WriteLine($"    Pressure Rating: {pipe.PressureRating}");
            Console.WriteLine($"    Length         : {pipe.LengthM} m");
        }

        Console.WriteLine($"\n--- EQUIPMENT ({doc.Equipment.Count}) ---");
        foreach (var e in doc.Equipment)
        {
            Console.WriteLine($"  {e.EquipmentType}");
            Console.WriteLine($"    Manufacturer: {e.Manufacturer}");
            Console.WriteLine($"    Model       : {e.Model}");
        }

        Console.WriteLine("\n--- INSULATION ---");
        if (doc.Insulation is null)
        {
            Console.WriteLine("  Not specified.");
        }
        else
        {
            Console.WriteLine($"  Material : {doc.Insulation.Material}");
            Console.WriteLine($"  Thickness: {doc.Insulation.ThicknessMm} mm");
        }

        Console.WriteLine($"\n--- TECHNICAL REQUIREMENTS ({doc.Requirements.Count}) ---");
        foreach (var r in doc.Requirements)
            Console.WriteLine($"  {r.Number}. [{r.Category}] {r.Text}");
    }

    private static void ShowValidation(IReadOnlyList<ValidationError> errors)
    {
        Console.WriteLine("\n--- VALIDATION ---");

        if (errors.Count == 0)
        {
            Console.WriteLine("  All validations passed.");
            return;
        }

        Console.WriteLine($"  {errors.Count} validation error(s):");
        foreach (var error in errors)
            Console.WriteLine($"  - {error}");
    }

    private static void ShowRequirementCheck(TechnicalDocument doc)
    {
        var insulationReq = doc.Requirements
            .FirstOrDefault(r => r.Category.Equals("Insulation", StringComparison.OrdinalIgnoreCase));

        if (insulationReq is null || doc.Pipes.Count == 0)
            return;

        Console.WriteLine("\n--- REQUIREMENT CHECK ---");
        Console.WriteLine($"  Rule: \"{insulationReq.Text}\"");

        foreach (var pipe in doc.Pipes)
        {
            if (pipe.DiameterMm > 80)
            {
                var status = doc.Insulation != null ? "PASS - insulation is specified" : "FAIL - insulation required but not found";
                Console.WriteLine($"  {pipe.Name} ({pipe.DiameterMm} mm) -> {status}");
            }
            else
            {
                Console.WriteLine($"  {pipe.Name} ({pipe.DiameterMm} mm) -> PASS - no insulation required");
            }
        }
    }

    private async Task RunQaLoop(string rawContent, CancellationToken cancellationToken)
    {
        Console.WriteLine("\n--- QUESTION & ANSWER ---");
        Console.WriteLine("Ask questions about the document. Type 'exit' to quit.\n");

        while (!cancellationToken.IsCancellationRequested)
        {
            Console.Write("Question: ");
            var question = Console.ReadLine()?.Trim();

            if (string.IsNullOrWhiteSpace(question))
                continue;

            if (question.Equals("exit", StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine("Goodbye.");
                break;
            }

            try
            {
                var answer = await analyzerService.AskQuestionAsync(rawContent, question, cancellationToken);
                Console.WriteLine($"Answer: {answer}\n");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] {ex.Message}\n");
            }
        }
    }
}
