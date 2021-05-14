﻿using System.Linq;
using System.Threading.Tasks;
using Disqord.Bot.Hosting;
using Disqord.Gateway;
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
        private const string ConfigPath = "./config.json";
        
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
                .ConfigureHostConfiguration(configuration => configuration.AddJsonFile(ConfigPath))
                .ConfigureServices(((context, services) =>
                {
                    services.AddSingleton<TypeLoaderService>();
                    services.AddSingleton<SearchService>();
                } ))
                .ConfigureDiscordBot((context, bot) =>
                {
                    bot.Token = context.Configuration["discord:token"];
                    // bot.OwnerIds = new[] {new Snowflake(Global.AuthorId)};
                    bot.Intents = GatewayIntents.All;
                })
                .Build();
            
            using (host)
            {
                await host.RunAsync();
            }
        }
    }
}