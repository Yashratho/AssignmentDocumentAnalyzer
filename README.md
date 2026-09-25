# Take-Home Assignment — C# + LLM Technical Document Analyzer

## Setup Instructions

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- An [OpenAI API key](https://platform.openai.com/api-keys)

### Configure API Key
Open `src/DocumentAnalyzer.ConsoleApp/appsettings.json` and replace the placeholder:
```json
{
  "LlmOptions": {
    "ApiKey": "YOUR_OPENAI_API_KEY_HERE"
  }
}
```
> **Never commit a real API key.** Use `appsettings.example.json` as a reference template.

You can also set it via environment variable (overrides appsettings.json):
```
set LlmOptions__ApiKey=sk-...
```

### How to Run
```bash
cd src/DocumentAnalyzer.ConsoleApp
dotnet run
```

Or to use a different document:
```bash
dotnet run -- --DocumentPath="C:/path/to/your_spec.txt"
```

---

## Architecture — Onion Architecture

The solution is structured as four separate .NET 8 projects following the **Onion Architecture** (also known as Clean Architecture). Dependencies flow strictly inward — outer layers depend on inner layers, never the reverse.

```
┌──────────────────────────────────────────────────┐
│           ConsoleApp  (Presentation)             │
│  Program.cs · ConsoleRunner · DI setup           │
├──────────────────────────────────────────────────┤
│           Infrastructure  (Adapters)             │
│  FileDocumentReader · OpenAiLlmService           │
│  PromptBuilder · LlmOptions                      │
├──────────────────────────────────────────────────┤
│           Application  (Use Cases)               │
│  IDocumentReader · ILlmService                   │
│  DocumentAnalyzerService · AnalysisResult        │
├──────────────────────────────────────────────────┤
│           Domain  (Core)                         │
│  TechnicalDocument · PipeSpecification           │
│  EquipmentSpecification · InsulationSpecification│
│  Requirement · DocumentValidator · Exceptions    │
└──────────────────────────────────────────────────┘
         ↑ Dependencies point inward only
```

### Layer responsibilities

| Layer | Project | Depends on |
|---|---|---|
| **Domain** | `DocumentAnalyzer.Domain` | Nothing (pure C#) |
| **Application** | `DocumentAnalyzer.Application` | Domain only |
| **Infrastructure** | `DocumentAnalyzer.Infrastructure` | Application + Domain |
| **ConsoleApp** | `DocumentAnalyzer.ConsoleApp` | All layers |

---

## Domain Models

All values in these models come from the LLM at runtime — **nothing is hard-coded**.

| Model | Fields |
|---|---|
| `TechnicalDocument` | ProjectInfo, Pipes, Equipment, Insulation, Requirements |
| `ProjectInfo` | ProjectName, Discipline, System |
| `PipeSpecification` | Name, Material, DiameterMm, PressureRating, LengthM |
| `EquipmentSpecification` | EquipmentType, Manufacturer, Model |
| `InsulationSpecification` | Material, ThicknessMm |
| `Requirement` | Number, Text, Category |

---

## Prompt Design

### Extraction Prompt
- **System prompt**: Instructs the LLM to act as a strict JSON extractor. Embeds the exact JSON schema. Explicitly states: "Return ONLY valid JSON — no markdown, no code blocks, no explanations."
- **Why JSON schema in the prompt?** Providing the exact expected schema in the system prompt dramatically reduces hallucinations and format errors compared to free-form descriptions.
- **Numeric extraction**: The prompt explicitly says to extract numeric values without units (e.g., "100 mm" → `100`). This allows domain validation (`diameter > 0`) and cross-checks (`diameter > 80`) to work as real numeric comparisons.
- **Defensive parsing**: Even with clear instructions, some LLMs occasionally wrap JSON in markdown code blocks. `OpenAiLlmService` strips ` ```json ... ``` ` defensively before deserializing.

### Q&A Prompt
- **System prompt**: "Answer using ONLY the provided document. If not found, say 'I cannot find this in the document.'" — strictly grounds the LLM in the document, preventing hallucinations from general knowledge.
- **No RAG/embeddings**: The full document text is sent with every Q&A call. This is appropriate for the size of technical specifications and requires no vector database.

### Response Format
JSON was chosen because:
1. It maps directly to C# types via `System.Text.Json`
2. It is unambiguous and machine-parseable
3. It allows strict validation after extraction

---

## Error Handling

| Scenario | Handling |
|---|---|
| Missing document file | `DocumentNotFoundException` → user-friendly console message |
| Empty document file | `DocumentEmptyException` → user-friendly console message |
| LLM API key invalid / API failure | `LlmServiceException` → console error with details |
| Network timeout | `OperationCanceledException` caught → `LlmServiceException` with timeout info |
| Malformed JSON from LLM | `JsonException` caught → `LlmServiceException` with raw response |
| Missing required fields (validation) | `DocumentValidator` reports all errors clearly |

---

## Validation Rules (from README)

All rules are implemented in `DocumentValidator.cs` in the Domain layer:

- ✅ Project name must not be empty
- ✅ Pipe diameter must be greater than zero
- ✅ Pipe material must not be empty when a pipe is present
- ✅ Insulation thickness must be greater than zero when insulation is specified
- ✅ Equipment type must not be empty
- ✅ Requirements must contain meaningful text (≥ 5 characters)

---

## Assumptions

1. The document is a `.txt` file encoded in UTF-8.
2. The `Input/technical_spec.txt` path is relative to the working directory when running.
3. The OpenAI `gpt-4o-mini` model is used by default (cost-efficient, sufficient for extraction tasks). This is configurable.
4. Insulation is document-wide (one block), not per-pipe — matching the structure of the provided sample document.
5. The document is small enough to fit within the LLM's context window (no chunking needed).
6. Q&A answers are generated fresh from the full document text on every question — no caching required.

---

## What I Would Improve With More Time

1. **Unit tests**: Add xUnit tests for `DocumentValidator` and mock-based tests for `DocumentAnalyzerService`.
2. **Retry logic**: Add Polly-based retry with exponential backoff for transient API failures.
3. **Streaming Q&A**: Use OpenAI streaming API to display Q&A answers word-by-word for a better UX.
4. **Multiple document support**: Accept a folder path and analyze multiple specs in batch.
5. **Output export**: Save the extracted `TechnicalDocument` as a JSON or CSV file.
6. **Azure OpenAI support**: Add a configurable provider switch (OpenAI vs Azure OpenAI).
