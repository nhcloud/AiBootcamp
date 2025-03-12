
namespace AiBootcamp.Sk;

public class Chat(AppSettings appSettings)
{
    public async Task RunAsync()
    {

        var builder = Kernel.CreateBuilder();
        builder.AddAzureOpenAIChatCompletion(appSettings.Deployment, appSettings.Endpoint,appSettings.ApiKey);
        var kernel = builder.Build();

        var chatService = kernel.GetRequiredService<IChatCompletionService>();
        var chatHistory = new ChatHistory();

        while (true)
        {
            Console.Write("Prompt:");
            var userInput = Console.ReadLine();
            if (!string.IsNullOrEmpty(userInput))
            {
                chatHistory.AddUserMessage(userInput);
                var completion = chatService.GetStreamingChatMessageContentsAsync(chatHistory,
                    executionSettings: new PromptExecutionSettings()
                    {
                        FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
                    },
                     kernel: kernel);
                var fullMessage = "";
                await foreach (var message in completion)
                {
                    Console.Write(message.Content);
                    fullMessage += message.Content;
                }
                chatHistory.AddAssistantMessage(fullMessage);
                Console.WriteLine();
            }
            else
            {
                Console.WriteLine("Input cannot be empty. Please enter a valid prompt.");
            }
        }
    }
}
