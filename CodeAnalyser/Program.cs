using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;

var config = new ConfigurationBuilder()
    .AddUserSecrets<Program>()
    .Build();

var builder = Kernel.CreateBuilder();

builder.AddAzureOpenAIChatCompletion(
    deploymentName: "gpt-4o",
    endpoint: config["AzureOpenAI:Endpoint"]!,
    apiKey: config["AzureOpenAI:ApiKey"]!
);

var kernel = builder.Build();

var result = await kernel.InvokePromptAsync(
    "Explain what a Roslyn syntax tree is in 3 sentences. Be technical but clear."
);

Console.WriteLine(result);
