
var configuration = new Microsoft.Extensions.Configuration.ConfigurationBuilder()
           .SetBasePath(Directory.GetCurrentDirectory())
           .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
           .Build();
var appSettings = configuration.GetSection("AppSettings").Get<AppSettings>();

if (appSettings == null)
{
    throw new Exception("AppSettings is null");
}

//await new Chat(appSettings).RunAsync();
await new ChatWithPlugin(appSettings).RunAsync();