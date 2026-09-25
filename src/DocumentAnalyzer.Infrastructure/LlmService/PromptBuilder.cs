namespace DocumentAnalyzer.Infrastructure.LlmService;

internal static class PromptBuilder
{
    public static string ExtractionSystemPrompt => """
        You are a technical document data extractor.
        Extract structured information from the provided technical specification and return ONLY valid JSON.
        Do not include markdown, code blocks, or any explanation — raw JSON only.

        Rules:
        - Extract numeric values without units: "100 mm" → 100, "25.5 m" → 25.5
        - If a section is absent, use null for objects and [] for arrays
        - Requirement categories must be one of: Temperature, Insulation, Equipment, Documentation, Safety, Other

        Return this exact JSON structure:
        {
          "projectInfo": {
            "projectName": "string",
            "discipline": "string",
            "system": "string"
          },
          "pipes": [
            {
              "name": "string",
              "material": "string",
              "diameterMm": 0.0,
              "pressureRating": "string",
              "lengthM": 0.0
            }
          ],
          "equipment": [
            {
              "equipmentType": "string",
              "manufacturer": "string",
              "model": "string"
            }
          ],
          "insulation": {
            "material": "string",
            "thicknessMm": 0.0
          },
          "requirements": [
            {
              "number": 1,
              "text": "string",
              "category": "string"
            }
          ]
        }
        """;

    public static string BuildExtractionUserPrompt(string documentContent) =>
        $"Extract all information from this technical document:\n\n{documentContent}";

    public static string QaSystemPrompt => """
        You are a technical document assistant.
        Answer the question using ONLY the information in the provided document.
        If the answer is not in the document, say: "I cannot find this information in the document."
        Be concise and factual.
        """;

    public static string BuildQaUserPrompt(string documentContent, string question) =>
        $"Document:\n{documentContent}\n\nQuestion: {question}";
}
