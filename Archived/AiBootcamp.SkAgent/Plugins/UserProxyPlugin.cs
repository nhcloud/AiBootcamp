using System.ComponentModel;

namespace AiBootcamp.SkAgent.Plugins;

public class UserProxyPlugin
{
    [KernelFunction("WaitForHumanDecisionAsync")]
    [Description("Present the approved content to the user and get their confirmation.")]
    [return: Description("The user's decision to accept or deny the content.")]
    public async Task<string> WaitForHumanDecisionAsync(string Information = "")
    {
        Console.WriteLine();
        Console.WriteLine("User Confirmation is required for the following: ");
        Helper.WriteLine(Information, ConsoleColor.Green);
        Console.WriteLine("Please type 'accept' to confirm or 'deny' to request changes:");

        string humanDecision = Console.ReadLine() ?? "accept";//"deny"

        return humanDecision;
    }
}


