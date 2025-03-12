
namespace AiBootcamp.SkAgent;
internal class Helper
{
    internal static void WriteLine(object? message, ConsoleColor color = ConsoleColor.Green)
    {
        var originalColor = Console.ForegroundColor;
        Console.ForegroundColor = color;
        Console.WriteLine(message);
        Console.ForegroundColor = originalColor;
    }
    internal static void WriteLineWithHighlight(object? message, ConsoleColor color = ConsoleColor.Green)
    {
        var originalColor = Console.ForegroundColor;
        var originalBackgroundColor = Console.BackgroundColor;
        Console.ForegroundColor = color;
        Console.BackgroundColor = ConsoleColor.Yellow;
        Console.WriteLine(message);
        Console.ForegroundColor = originalColor;
        Console.BackgroundColor = originalBackgroundColor;
    }
    internal static void Write(object? message, ConsoleColor color = ConsoleColor.Green)
    {
        var originalColor = Console.ForegroundColor;
        Console.ForegroundColor = color;
        Console.Write(message);
        Console.ForegroundColor = originalColor;
    }
    internal static string GetPrompt()
    {
        Console.WriteLine();
        Console.Write("User (type 'exit' to exit): ");
        return Console.ReadLine() ?? string.Empty;
    }
    public static async Task PublishToBlogSiteAsync()
    {
        WriteLine("Publishing content to blog site...", ConsoleColor.Green);
        await Task.Delay(2000); 
        WriteLine("Blog content published successfully!", ConsoleColor.Green);
        Console.ResetColor();  
    }
}
