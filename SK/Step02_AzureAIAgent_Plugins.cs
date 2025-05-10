using Plugins;
using AgentThread = Microsoft.SemanticKernel.Agents.AgentThread;

namespace SK;

/// <summary>
/// Demonstrate creation of <see cref="AzureAIAgent"/> with a <see cref="KernelPlugin"/>,
/// and then eliciting its response to explicit user messages.
/// </summary>
public class Step02_AzureAIAgent_Plugins
{
    public static async Task RunAsync()
    {
        AgentsClient agentsClient = new(Utils.ConnectionString, new AzureCliCredential());
        // Define the agent
        AzureAIAgent agent = await CreateAzureAgentAsync(agentsClient,
                plugin: KernelPluginFactory.CreateFromType<MenuPlugin>(),
                instructions: "Answer questions about the menu.",
                name: "Host");
        var meta = new Dictionary<string, string>
        {
            { "sksample", bool.TrueString }
        };
        // Create a thread for the agent conversation.
        AgentThread thread = new AzureAIAgentThread(agentsClient, metadata: meta);

        // Respond to user input
        try
        {
            await Utils.InvokeAgentAsync(agent, thread, "Hello");
            await Utils.InvokeAgentAsync(agent, thread, "What is the special soup and its price?");
            await Utils.InvokeAgentAsync(agent, thread, "What is the special drink and its price?");
            await Utils.InvokeAgentAsync(agent, thread, "Thank you");
        }
        finally
        {
            await thread.DeleteAsync();
            //await agentClient.DeleteAgentAsync(agent.Id);
        }
        await UseAzureAgentWithPluginEnumParameter(agentsClient);
    }


    public static async Task UseAzureAgentWithPluginEnumParameter(AgentsClient agentClient)
    {
        // Define the agent
        AzureAIAgent agent = await CreateAzureAgentAsync(agentClient, plugin: KernelPluginFactory.CreateFromType<WidgetFactory>());

        var meta = new Dictionary<string, string>
        {
            { "sksample", bool.TrueString }
        };
        // Create a thread for the agent conversation.
        AgentThread thread = new AzureAIAgentThread(agentClient, metadata: meta);

        // Respond to user input
        try
        {
            await Utils.InvokeAgentAsync(agent, thread, "Create a beautiful red colored widget for me.");
        }
        finally
        {
            await thread.DeleteAsync();
            await agentClient.DeleteAgentAsync(agent.Id);
        }
    }

    private static async Task<AzureAIAgent> CreateAzureAgentAsync(AgentsClient agentClient, KernelPlugin plugin, string? instructions = null, string? name = null)
    {
        // Define the agent
        Azure.AI.Projects.Agent definition = await agentClient.CreateAgentAsync(
            Utils.AIModel,
            name,
            null,
            instructions);

        AzureAIAgent agent = new(definition, agentClient);

        // Add to the agent's Kernel
        if (plugin != null)
        {
            agent.Kernel.Plugins.Add(plugin);
        }

        return agent;
    }

}
