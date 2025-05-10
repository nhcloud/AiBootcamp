namespace AiBootcamp.AgAgent.Agents

public class SimpleSemanticKernelChatAgent(AppSettings appSettings)
{
    public  async Task RunAsync()
    {
        #region Create_Kernel

        var kernel = Kernel.CreateBuilder()
        .AddAzureOpenAIChatCompletion(appSettings.Deployment, appSettings.Endpoint, appSettings.ApiKey)
        .Build();

        #endregion Create_Kernel

        #region Create_ChatCompletionAgent
        // The built-in ChatCompletionAgent from semantic kernel.

        var chatAgent = new ChatCompletionAgent()
        {
            Kernel = kernel,
            Name = "assistant",
            Description = "You are a helpful AI assistant",
        };

        #endregion Create_ChatCompletionAgent

        #region Create_SemanticKernelChatCompletionAgent
        var messageConnector = new SemanticKernelChatMessageContentConnector();
        var skAgent = new SemanticKernelChatCompletionAgent(chatAgent)
            .RegisterMiddleware(messageConnector) // register message connector so it support AutoGen built-in message types like TextMessage.
            .RegisterPrintMessage(); // pretty print the message to the console
        #endregion Create_SemanticKernelChatCompletionAgent

        #region Send_Message
        await skAgent.SendAsync("Hey tell me a long tedious joke");
        #endregion Send_Message
    }
}
