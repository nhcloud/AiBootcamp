using AgentThread = Microsoft.SemanticKernel.Agents.AgentThread;

namespace SK;

internal class Step01_AzureAIAgent
{
    internal async static Task RunAsync()
    {
        string generateStoryYaml = EmbeddedResource.Read("GenerateStory.yaml");
        PromptTemplateConfig templateConfig = KernelFunctionYaml.ToPromptTemplateConfig(generateStoryYaml);
        AgentsClient agentsClient = new(Utils.ConnectionString, new AzureCliCredential());

        Azure.AI.Projects.Agent definition = await agentsClient.CreateAgentAsync(Utils.AIModel, templateConfig.Name, templateConfig.Description, templateConfig.Template);
        AzureAIAgent agent = new(
            definition,
            agentsClient,
            templateFactory: new KernelPromptTemplateFactory(),
            templateFormat: PromptTemplateConfig.SemanticKernelTemplateFormat)
        {
            Arguments = new()
        {
            { "topic", "Dog" },
            { "length", "3" }
        }
        };
        var sampleMeta = new Dictionary<string, string>
    {
        { "sksample", bool.TrueString }
    };
        AgentThread thread = new AzureAIAgentThread(agentsClient, metadata: sampleMeta);
        try
        {
            // Invoke the agent with the default arguments.
            await Utils.InvokeAgentAsync(agent, thread, (KernelArguments?)null);

            // Invoke the agent with the override arguments.
            await Utils.InvokeAgentAsync(agent, thread,
                new KernelArguments()
                {
                { "topic", "Cat" },
                { "length", "3" },
                });
        }
        finally
        {
            await thread.DeleteAsync();
            await agentsClient.DeleteAgentAsync(agent.Id);
        }
    }
}
