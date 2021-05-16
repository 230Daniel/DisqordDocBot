using System.Linq;
using System.Threading.Tasks;
using Disqord.Bot.Hosting;
using Disqord.Gateway;
using DisqordDocBot.EFCore;
using DisqordDocBot.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;

namespace DisqordDocBot
{
    class Program
    {
        public static async Task Main(string[] args)
        {
            var host = new HostBuilder()
                .ConfigureLogging(x =>
                {
                    var logger = new LoggerConfiguration()
                        .MinimumLevel.Information()
                        .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                        .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}", theme: AnsiConsoleTheme.Code)
                        .CreateLogger();
                    
                    x.AddSerilog(logger, true);
                    
                    x.Services.Remove(x.Services.First(x => x.ServiceType == typeof(ILogger<>)));
                    x.Services.AddSingleton(typeof(ILogger<>), typeof(Logger<>));
                })
                .ConfigureHostConfiguration(configuration => configuration.AddJsonFile(Global.ConfigPath))
                .ConfigureServices(((context, services) =>
                {
                    services.AddDbContextFactory<DatabaseContext>();
                    services.AddSingleton<TypeLoaderService>();
                    services.AddSingleton<SearchService>();
                    services.AddSingleton<TagService>();
                } ))
                .ConfigureDiscordBot((context, bot) =>
                {
                    bot.Token = context.Configuration["discord:token"];
                    bot.Intents = GatewayIntents.Recommended;
                    bot.Prefixes = context.Configuration.GetSection("discord:prefixes").Get<string[]>();
                })
                .Build();
            
            using (host)
            {
                await host.RunAsync();
            }
        }
    }
}