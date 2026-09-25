using DocumentAnalyzer.Application.Interfaces;
using DocumentAnalyzer.Application.Services;
using DocumentAnalyzer.ConsoleApp;
using DocumentAnalyzer.Domain.Validation;
using DocumentAnalyzer.Infrastructure.FileReading;
using DocumentAnalyzer.Infrastructure.LlmService;
using DocumentAnalyzer.Infrastructure.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

// 1. Load Configuration
var config = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false)
    .Build();

// 2. Setup Dependency Injection
var services = new ServiceCollection()
    .Configure<AiSettings>(config.GetSection(AiSettings.SectionName))
    .AddSingleton<DocumentValidator>()
    .AddSingleton<IDocumentReader, FileDocumentReader>()
    .AddSingleton<ILlmService, OpenAiLlmService>()
    .AddSingleton<DocumentAnalyzerService>()
    .AddSingleton<ConsoleRunner>()
    .BuildServiceProvider();

// 3. Resolve Path and Run
var documentPath = Path.Combine(AppContext.BaseDirectory, config["DocumentPath"] ?? "Input/technical_spec.txt");
var runner = services.GetRequiredService<ConsoleRunner>();

await runner.RunAsync(documentPath);
