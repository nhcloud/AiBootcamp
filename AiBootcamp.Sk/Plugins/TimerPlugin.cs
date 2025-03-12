using System.ComponentModel;

namespace AiBootcamp.Sk.Plugins;

public class TimePlugin
{
    [KernelFunction("GetTime")]
    [Description("Get the current time.")]
    [return: Description("The current time.")]
    public static DateTime GetCurrentTime()
    {
        return DateTime.Now;
    }
}