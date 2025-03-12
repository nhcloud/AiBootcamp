using Bootcamp.Common;
using Microsoft.SemanticKernel.Agents.Chat;
using System.ComponentModel;

var builder = Kernel.CreateBuilder().AddAzureOpenAIChatCompletion(AppSettings.ModelId, AppSettings.Endpoint, AppSettings.ApiKey);
var kernel = builder.Build();

await ContentApprovalProcessAsync(kernel);

Console.ReadLine();

async Task ContentApprovalProcessAsync(Kernel kernel)
{
    // Initialize the agents
    string terminationKey = "Finalize";

    // Define the instructions for each agent
    var agents = InitializeAgents(kernel, terminationKey);

    // Create the chat group with the agents
    AgentGroupChat blogPostGroupChat = new(agents["WriterAgent"], agents["ApproverAgent"], agents["UserProxyAgent"])
    {
        ExecutionSettings = new()
        {
            TerminationStrategy = new ApprovalTerminationStrategy(terminationKey)
            {
                Agents = [agents["ApproverAgent"]],
                MaximumIterations = 3 // Maximum iterations to simulate rejection and re-write
            }
        }
    };

    // Start the conversation loop
    Console.Write("Enter the initial content for the blog post (type 'exit' to quit):");
    var initialContent = Console.ReadLine();

    // Add the initial content from the writer
    blogPostGroupChat.AddChatMessage(new ChatMessageContent(AuthorRole.User, initialContent));

    // Start the chat
    await foreach (var content in blogPostGroupChat.InvokeAsync())
    {
        Console.WriteLine($"## {content.Role} - {content.AuthorName ?? "*"} ##");
        Console.WriteLine($"{content.Content}");
        Console.WriteLine();
    }

    // Simulate random approval/rejection process
    bool isApproved = false;
    int iteration = 0;

    // Simulate random rejection or approval until content is approved or max iterations are reached
    while (!isApproved && iteration < 5)
    {
        iteration++;

        // Randomly decide if the approver approves or rejects
        Random rand = new Random();
        string approvalStatus = rand.Next(2) == 0 ? "rejected" : "approved";

        Console.WriteLine($"Approver {approvalStatus} the post. Iteration {iteration}...");

        if (approvalStatus == "approved")
        {
            isApproved = true;
            await Helper.PublishToBlogSiteAsync();
            //await UserConfirmationAndPublishAsync(agents["UserProxyAgent"], "Final blog content ready for publishing.");
        }
        else
        {
            Helper.Print("Content not approved. Rewriting the blog post...", ConsoleColor.Red);
            string newContent = $"Revised content after rejection, iteration {iteration}.";
            blogPostGroupChat.AddChatMessage(new ChatMessageContent(AuthorRole.User, newContent));

            // Simulate re-writing the content
            await foreach (var content in blogPostGroupChat.InvokeAsync())
            {
                Console.WriteLine($"## {content.Role} - {content.AuthorName ?? "*"} ##");
                Console.WriteLine($"{content.Content}");
                Console.WriteLine();
            }
        }
    }

    if (!isApproved)
    {
        Helper.Print("Max iterations reached. Content still not approved.", ConsoleColor.Red);
    }
}

//async Task UserConfirmationAndPublishAsync(ChatCompletionAgent userProxyAgent, string finalContent)
//{
//    string userConfirmation = await userProxyAgent.GetUserContentPublishConfirmationAsync(finalContent);
//    if (userConfirmation.ToLower() == "approve")
//    {
//        Helper.Print("Blog content approved and ready to publish!", ConsoleColor.Green);
//        await Helper.PublishToBlogSiteAsync();
//    }
//    else
//    {
//        Helper.Print("Content not approved. Rewriting the blog post...", ConsoleColor.Red);
//    }
//}



static Dictionary<string, ChatCompletionAgent> InitializeAgents(Kernel kernel, string terminationKey)
{
    string writerInstructions = """
            You are a Writer Agent, tasked with drafting blog content related to travel and bookings.
            Write the initial draft and provide it for approval.
        """;

    string approverInstructions = """
            You are an Approver Agent, tasked with reviewing the blog content submitted by the writer.
            You can either approve or reject the content. If rejected, provide feedback for re-writing.
            Respond with 'approved' or 'rejected' to indicate your decision.
        """;

    string userProxyInstructions = """
            You are an agent that presents the final content to the user and gets their confirmation.
            If the user approves the content, send it for publishing; otherwise, notify the writer for re-writing.
        """;

    ChatCompletionAgent writerAgent = CreateBasicAgent("WriterAgent", kernel, writerInstructions, "Content Writer");
    ChatCompletionAgent approverAgent = CreateBasicAgent("ApproverAgent", kernel, approverInstructions, "Content Approver");
    ChatCompletionAgent userProxyAgent = CreateBasicAgent("UserProxyAgent", kernel, userProxyInstructions, "User Proxy Agent, you collect user input and send it to the other agents", new UserInputsProxy());

    return new Dictionary<string, ChatCompletionAgent>
    {
        { "WriterAgent", writerAgent },
        { "ApproverAgent", approverAgent },
        { "UserProxyAgent", userProxyAgent }
    };
}

static ChatCompletionAgent CreateBasicAgent(string agentName, Kernel agentKernel, string agentInstructions, string agentDescription, object? plugin = null)
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

public class UserInputsProxy
{
    [KernelFunction("GetUserContentPublishConfirmationAsync")]
    [Description("Present the content to the user and get their confirmation.")]
    public async Task<string> GetUserContentPublishConfirmationAsync(string Information)
    {
        Console.WriteLine();
        Console.WriteLine("User Confirmation is required for the following: ");
        Helper.Print(Information, ConsoleColor.Green);
        Console.WriteLine("Please type 'approve' to confirm or 'reject' to request changes:");

        string userInput = Console.ReadLine() ?? "No comments";
        return await Task.FromResult(userInput);
    }
}
public static class Helper
{
    public static async Task PublishToBlogSiteAsync()
    {
        Print("Publishing content to blog site...", ConsoleColor.Green);
        await Task.Delay(2000); // Simulate the publishing delay
        Print("Blog content published successfully!", ConsoleColor.Green);
        Console.ResetColor();  // Reset the text color to default
    }

   public static void Print(string message, ConsoleColor color)
    {
        Console.ForegroundColor = color;
        Console.WriteLine(message);
        Console.ResetColor();
    }
}
public sealed class ApprovalTerminationStrategy(string terminationKey) : TerminationStrategy
{
    // Terminate when the final message contains the term including the termination key.
    protected override Task<bool> ShouldAgentTerminateAsync(Agent agent, IReadOnlyList<ChatMessageContent> history, CancellationToken cancellationToken)
        => Task.FromResult(history[history.Count - 1].Content?.Contains(terminationKey, StringComparison.OrdinalIgnoreCase) ?? false);
}
//public static class ChatCompletionAgentExtensions
//{
//    public static async Task<string> GetUserContentPublishConfirmationAsync(this ChatCompletionAgent agent, string information)
//    {
//        Console.WriteLine();
//        Console.WriteLine("User Confirmation is required for the following: ");
//        Console.WriteLine(information);
//        Console.WriteLine("Please type 'approve' to confirm or 'reject' to request changes:");

//        string userInput = Console.ReadLine() ?? "No comments";
//        return await Task.FromResult(userInput);
//    }
//}
