﻿namespace AiBootcamp.SkAgent.Agents;

public class SimpleAgentWithPlugin
{
    private static Kernel _kernel;
    private readonly AppSettings _appSettings;

    public SimpleAgentWithPlugin(AppSettings appSettings)
    {
        _appSettings = appSettings;
        _kernel = Kernel.CreateBuilder()
            .AddAzureOpenAIChatCompletion(_appSettings.Deployment, _appSettings.Endpoint, _appSettings.ApiKey)
            .Build();
        _kernel.Plugins.AddFromObject(new WeatherPlugin(_appSettings));
    }

    public async Task RunAsync()
    {
        ChatHistory history = new();

        ChatCompletionAgent agent = new()
        {
            Name = "TravelWriter",
            Description = File.ReadAllText(Path.Combine("Instructions", "TravelWriterDescription.txt")),
            Instructions = File.ReadAllText(Path.Combine("Instructions", "TravelWriterInstruction.txt")),
            Kernel = _kernel,
            Arguments = new KernelArguments(new AzureOpenAIPromptExecutionSettings()
            {
                FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
            })
        };

        while (true)
        {
            var userInput = Helper.GetPrompt();
            if (userInput == "exit")
            {
                break;
            }
            history.Add(new ChatMessageContent(AuthorRole.User, userInput));
            await foreach (var message in agent.InvokeStreamingAsync(history))
            {
                Helper.Write(message);
            }
        }
    }
}

