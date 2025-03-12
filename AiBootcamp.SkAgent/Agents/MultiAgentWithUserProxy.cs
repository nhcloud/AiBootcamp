namespace AiBootcamp.SkAgent.Agents;

public class MultiAgentWithUserProxy
{
    private static Kernel _kernel;
    private readonly AppSettings _appSettings;
    private const string terminationKey = "<Published>";
    private const string terminationAgentName = "Publisher";
    public MultiAgentWithUserProxy(AppSettings appSettings)
    {
        _appSettings = appSettings;
        _kernel = Kernel.CreateBuilder()
            .AddAzureOpenAIChatCompletion(_appSettings.Deployment, _appSettings.Endpoint, _appSettings.ApiKey)
            .Build();
        _kernel.Plugins.AddFromObject(new WeatherPlugin(_appSettings));
        _kernel.Plugins.AddFromObject(new PublisherPlugin(_appSettings));
    }
    public async Task RunAsync()
    {
        var agents = InitializeAgents(_kernel, terminationKey);

        var groupChat = new AgentGroupChat(agents["TravelWriter"], agents["Reviewer"], agents["UserProxy"], agents["Publisher"])
        {
            ExecutionSettings = new()
            {
                TerminationStrategy = new ApprovalTerminationStrategy(terminationKey)
                {
                    Agents = [agents[terminationAgentName]],
                    MaximumIterations = 5
                }
            }
        };

        // Start the agent workflow, passing in initial content or prompts
        string initialPrompt = Helper.GetPrompt();
        if (!initialPrompt.Equals("exit", StringComparison.OrdinalIgnoreCase))
        {
            groupChat.AddChatMessage(new ChatMessageContent(AuthorRole.User, initialPrompt));

            // Execute the flow: let the agents handle the interaction
            await ExecuteAgentFlowAsync(groupChat, agents);
        }
    }
    private async Task ExecuteAgentFlowAsync(AgentGroupChat groupChat, Dictionary<string, ChatCompletionAgent> agents)
    {
        bool isPublished = false;

        while (!isPublished)
        {
            // Let the agents handle the interaction (writer -> Reviewer -> human -> publisher)
            await foreach (var content in groupChat.InvokeAsync())
            {
                Helper.WriteLineWithHighlight($"## {content.Role} - {content.AuthorName ?? "*"} ##", ConsoleColor.Black);
                Console.WriteLine($"{content.Content}");
                Console.WriteLine();
            }

            await foreach (var content in groupChat.InvokeAsync())
            {
                Helper.WriteLineWithHighlight($"## {content.Role} - {content.AuthorName ?? "*"} ##", ConsoleColor.Black);
                Console.WriteLine($"{content.Content}");
                Console.WriteLine();
            }
            isPublished = true;
            // Get the chat history of publisher
            ChatMessageContent[] agentHistory = await groupChat.GetChatMessagesAsync(agents[terminationAgentName]).ToArrayAsync();
            if (agentHistory.Length > 0 && agentHistory[0]?.Content?.Contains("<notpublished>") == true)
            {
                Console.WriteLine("Resetting the writer flow...");
                await groupChat.ResetAsync();
                isPublished = false;
            }
        }
    }


    static ChatCompletionAgent CreateAgent(string agentName, Kernel agentKernel, string agentInstructions, string agentDescription, object? plugin = null)
    {
        var functionChoiceBehavior = FunctionChoiceBehavior.None();
        if (plugin != null)
        {
            agentKernel = agentKernel.Clone();
            agentKernel.Plugins.AddFromObject(plugin);
            functionChoiceBehavior = FunctionChoiceBehavior.Auto();
        }

        return new ChatCompletionAgent()
        {
            Instructions = agentInstructions,
            Name = agentName,
            Kernel = agentKernel,
            Description = agentDescription,
            Arguments = new KernelArguments(new AzureOpenAIPromptExecutionSettings()
            {
                FunctionChoiceBehavior = functionChoiceBehavior
            })
        };
    }


    static Dictionary<string, ChatCompletionAgent> InitializeAgents(Kernel kernel, string terminationKey)
    {
        string writerInstructions = File.ReadAllText(Path.Combine("Instructions", "TravelWriterInstruction.txt"));
        string reviewerInstructions = File.ReadAllText(Path.Combine("Instructions", "ReviewerInstruction.txt"));
        string userProxyInstructions = File.ReadAllText(Path.Combine("Instructions", "UserProxyInstruction.txt"));
        string publisherInstructions = File.ReadAllText(Path.Combine("Instructions", "PublisherInstruction.txt"));

        string writerDescription = File.ReadAllText(Path.Combine("Instructions", "TravelWriterDescription.txt"));
        string ReviewerDescription = File.ReadAllText(Path.Combine("Instructions", "ReviewerDescription.txt"));
        string proxyDescription = File.ReadAllText(Path.Combine("Instructions", "UserProxyDescription.txt"));
        string publisherDescription = File.ReadAllText(Path.Combine("Instructions", "PublisherDescription.txt"));

        ChatCompletionAgent writerAgent = CreateAgent("TravelWriter", kernel, writerInstructions, writerDescription);
        ChatCompletionAgent reviewerAgent = CreateAgent("Reviewer", kernel, reviewerInstructions, ReviewerDescription);
        ChatCompletionAgent userProxyAgent = CreateAgent("UserProxy", kernel, userProxyInstructions, proxyDescription, new UserProxyPlugin());
        ChatCompletionAgent publisherAgent = CreateAgent("Publisher", kernel, publisherInstructions, publisherDescription);

        return new Dictionary<string, ChatCompletionAgent>
        {
            { "TravelWriter", writerAgent },
            { "Reviewer", reviewerAgent },
            { "UserProxy", userProxyAgent },
            { "Publisher", publisherAgent }
        };
    }
}