var configuration = new ConfigurationBuilder()
           .SetBasePath(Directory.GetCurrentDirectory()) 
           .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
           .Build();
var appSettings = configuration.GetSection("AppSettings").Get<AppSettings>();

if(appSettings==null)
{
    throw new Exception("AppSettings is null");
}
//Basic Agent
//await new SimpleAgent(appSettings).RunAsync();
//await new SimpleAgentWithPlugin(appSettings).RunAsync();
//await new MultiAgent(appSettings).RunAsync();
await new MultiAgentWithUserProxy(appSettings).RunAsync();
