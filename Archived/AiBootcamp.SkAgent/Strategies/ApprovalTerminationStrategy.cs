using Microsoft.SemanticKernel.Agents.Chat;

namespace AiBootcamp.SkAgent.Strategies;

public sealed class ApprovalTerminationStrategy(string terminationKey) : TerminationStrategy
{
    protected override Task<bool> ShouldAgentTerminateAsync(Agent agent, IReadOnlyList<ChatMessageContent> history, CancellationToken cancellationToken)
        => Task.FromResult(history[history.Count - 1].Content?.Contains(terminationKey, StringComparison.OrdinalIgnoreCase) ?? false);
}
