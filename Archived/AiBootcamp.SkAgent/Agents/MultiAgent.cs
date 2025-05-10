namespace AiBootcamp.SkAgent.Agents;

public class MultiAgent
{
    private static Kernel _kernel;
    private readonly AppSettings _appSettings;
    private const string terminationKey = "approved";
    public MultiAgent(AppSettings appSettings)
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

        var agents = InitializeAgents(_kernel, terminationKey);

        AgentGroupChat chat =
           new(agents["TravelWriter"], agents["Reviewer"])
           {
               ExecutionSettings =
                   new()
                   {
                       TerminationStrategy =
                           new ApprovalTerminationStrategy(terminationKey)
                           {
                               Agents = [agents["Reviewer"]],
                               MaximumIterations = 3
                           }
                   }
           };
        var userInput = Helper.GetPrompt();
        chat.AddChatMessage(new ChatMessageContent(AuthorRole.User, userInput));

        await foreach (var content in chat.InvokeAsync())
        {
            Console.WriteLine($"## {content.Role} - {content.AuthorName ?? "*"} ##");
            Console.WriteLine($"{content.Content}");
            Console.WriteLine();
        }
    }
    static ChatCompletionAgent CreateAgent(string agentName, Kernel agentKernel, string agentInstructions, string agentDescription, object? plugin = null)
    {
        var myFunctionChoiceBehavior = FunctionChoiceBehavior.None();
        if (plugin != null)
        {
            agentKernel = agentKernel.Clone();
            agentKernel.Plugins.AddFromObject(plugin);
            myFunctionChoiceBehavior = FunctionChoiceBehavior.Auto();
        }

        return new ChatCompletionAgent()
        {
            Instructions = agentInstructions,
            Name = agentName,
            Kernel = agentKernel,
            Description = agentDescription,
            Arguments = new KernelArguments(new AzureOpenAIPromptExecutionSettings()
            {
                FunctionChoiceBehavior = myFunctionChoiceBehavior
            })
        };
    }
    static Dictionary<string, ChatCompletionAgent> InitializeAgents(Kernel kernel, string terminationKey)
    {
        string writerInstructions = File.ReadAllText(Path.Combine("Instructions", "TravelWriterInstruction.txt"));
        string reviewerInstructions = File.ReadAllText(Path.Combine("Instructions", "ReviewerInstruction.txt"));

        string writerDescription = File.ReadAllText(Path.Combine("Instructions", "TravelWriterDescription.txt"));
        string ReviewerDescription = File.ReadAllText(Path.Combine("Instructions", "ReviewerDescription.txt"));

        ChatCompletionAgent writerAgent = CreateAgent("TravelWriter", kernel, writerInstructions, writerDescription);
        ChatCompletionAgent reviewerAgent = CreateAgent("Reviewer", kernel, reviewerInstructions, ReviewerDescription);

        return new Dictionary<string, ChatCompletionAgent>
        {
            { "TravelWriter", writerAgent },
            { "Reviewer", reviewerAgent }
        };
    }
}
