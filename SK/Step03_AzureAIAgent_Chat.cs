
using Microsoft.SemanticKernel.Agents.Chat;
using Microsoft.SemanticKernel.ChatCompletion;
using Agent = Azure.AI.Projects.Agent;

namespace SK;

/// <summary>
/// Demonstrate creation of <see cref="AgentChat"/> with <see cref="AgentGroupChatSettings"/>
/// that inform how chat proceeds with regards to: Agent selection, chat continuation, and maximum
/// number of agent interactions.
/// </summary>
public class Step03_AzureAIAgent_Chat
{

    private static string ReviewerName = "ArtDirector";
    private static string ReviewerInstructions =
        """
        You are an art director who has opinions about copywriting born of a love for David Ogilvy.
        The goal is to determine if the given copy is acceptable to print.
        If so, state that it is approved.  Do not use the word "approve" unless you are giving approval.
        If not, provide insight on how to refine suggested copy without example.
        """;

    private static string CopyWriterName = "CopyWriter";
    private static string CopyWriterInstructions =
        """
        You are a copywriter with ten years of experience and are known for brevity and a dry humor.
        The goal is to refine and decide on the single best copy as an expert in the field.
        Only provide a single proposal per response.
        You're laser focused on the goal at hand.
        Don't waste time with chit chat.
        Consider suggestions when refining an idea.
        """;

    public static async Task RunAsync()
    {

        AgentsClient agentsClient = new(Utils.ConnectionString, new AzureCliCredential());
        // Define the agents
        Agent reviewerModel = await agentsClient.CreateAgentAsync(
            Utils.AIModel,
            ReviewerName,
            null,
            ReviewerInstructions);
        AzureAIAgent agentReviewer = new(reviewerModel, agentsClient);
        Agent writerModel = await agentsClient.CreateAgentAsync(
            Utils.AIModel,
            CopyWriterName,
            null,
            CopyWriterInstructions);
        AzureAIAgent agentWriter = new(writerModel, agentsClient);

        // Create a chat for agent interaction.
        AgentGroupChat chat =
            new(agentWriter, agentReviewer)
            {
                ExecutionSettings =
                    new()
                    {
                        // Here a TerminationStrategy subclass is used that will terminate when
                        // an assistant message contains the term "approve".
                        TerminationStrategy =
                            new ApprovalTerminationStrategy()
                            {
                                // Only the art-director may approve.
                                Agents = [agentReviewer],
                                // Limit total number of turns
                                MaximumIterations = 10,
                            }
                    }
            };

        try
        {
            // Invoke chat and display messages.
            ChatMessageContent input = new(AuthorRole.User, "concept: maps made out of egg cartons.");
            chat.AddChatMessage(input);
            Utils.WriteAgentChatMessage(input);

            await foreach (ChatMessageContent response in chat.InvokeAsync())
            {
                Utils.WriteAgentChatMessage(response);
            }

            Console.WriteLine($"\n[IS COMPLETED: {chat.IsComplete}]");
        }
        finally
        {
            await chat.ResetAsync();
        }
    }
}

public sealed class ApprovalTerminationStrategy : TerminationStrategy
{
    // Terminate when the final message contains the term "approve"
    protected override Task<bool> ShouldAgentTerminateAsync(Microsoft.SemanticKernel.Agents.Agent agent, IReadOnlyList<ChatMessageContent> history, CancellationToken cancellationToken)
        => Task.FromResult(history[history.Count - 1].Content?.Contains("approve", StringComparison.OrdinalIgnoreCase) ?? false);
}
