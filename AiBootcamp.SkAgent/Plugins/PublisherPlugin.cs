using System.ComponentModel;


namespace AiBootcamp.SkAgent.Plugins;

public class PublisherPlugin(AppSettings appSettings)
{

    [KernelFunction("Publisher")]
    [Description("Publish the content")]
    [return: Description("published.")]
    public async Task<string> PublishContentAsync(string content)
    {
        Helper.WriteLine("Publishing content to blog site...", ConsoleColor.Green);
        return "published.";
    }
}