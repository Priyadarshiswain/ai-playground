using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using System.ComponentModel;

var config = new ConfigurationBuilder()
    .AddUserSecrets<Program>()
    .Build();

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

var result = await kernel.InvokePromptAsync(
    "Use the code analyser tool to analyse the file at path 'CodeAnalyserPlugin.cs' and tell me what classes and methods it contains",
    new KernelArguments(executionSettings)
);

Console.WriteLine(result);