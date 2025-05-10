// Copyright (c) Microsoft Corporation. All rights reserved.
// Create_Semantic_Kernel_Agent.cs

namespace AiBootcamp.AgAgent.Agents;

public class SimpleSemanticKernelAgent(AppSettings appSettings)
{
    public async Task RunAsync()
    {
     
        var kernel = Kernel.CreateBuilder()
            .AddAzureOpenAIChatCompletion(appSettings.Deployment, appSettings.Endpoint, appSettings.ApiKey)
            .Build();

        var skAgent = new SemanticKernelAgent(
            kernel: kernel,
            name: "assistant",
            systemMessage: "You are a helpful AI assistant")
            .RegisterMessageConnector() // register message connector so it support AutoGen built-in message types like TextMessage.
            .RegisterPrintMessage(); // pretty print the message to the console

        var input=Helper.GetPrompt();
        await skAgent.SendAsync(input);
    }
}
