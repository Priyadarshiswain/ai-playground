using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using CodeAnalyser.Services;
using System.ComponentModel;

var config = new ConfigurationBuilder()
    .AddUserSecrets<Program>()
    .Build();

// MongoDB
var mongoStore = new MongoMethodStore(
    config["MongoDB:ConnectionString"] ?? "mongodb://localhost:27017"
);

var builder = Kernel.CreateBuilder();

builder.AddAzureOpenAIChatCompletion(
    deploymentName: "gpt-4o",
    endpoint: config["AzureOpenAI:Endpoint"]!,
    apiKey: config["AzureOpenAI:ApiKey"]!
);

builder.Plugins.AddFromType<CodeAnalyserPlugin>();

var kernel = builder.Build();

var executionSettings = new Microsoft.SemanticKernel.Connectors.AzureOpenAI.AzureOpenAIPromptExecutionSettings
{
    FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
};

var filePath = Path.GetFullPath(
    Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
        "..", "..", "..", "CodeAnalyserPlugin.cs")
);

var result = await kernel.InvokePromptAsync(
    $"In the file '{filePath}' show me the dependency graph for the AnalyseCode method only",
    new KernelArguments(executionSettings)
);

Console.WriteLine(result);

// Compute file hash
var fileContent = await File.ReadAllTextAsync(filePath);
var fileHash = Convert.ToHexString(
    System.Security.Cryptography.SHA256.HashData(
        System.Text.Encoding.UTF8.GetBytes(fileContent)
    )
);

// Build the document from what Roslyn already knows
var methodDoc = new CodeAnalyser.Models.MethodDocument
{
    FilePath = filePath,
    FileHash = fileHash,
    ClassName = "CodeAnalyserPlugin",
    MethodName = "AnalyseCode",
    AiDescription = result.ToString(),
    AiRiskLevel = "Medium"
};

// Save to MongoDB
await mongoStore.UpsertMethodAsync(methodDoc);
Console.WriteLine("✅ Method saved to MongoDB.");