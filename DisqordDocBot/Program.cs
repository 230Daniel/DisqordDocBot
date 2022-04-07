using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Disqord.Bot.Hosting;
using Disqord.Gateway;
using DisqordDocBot.EFCore;
using DisqordDocBot.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;

namespace DisqordDocBot
{
    internal class Program
    {
        public static async Task Main(string[] args)
        {
            var host = new HostBuilder()
                .UseSystemd()
                .ConfigureLogging(x =>
                {
                    var logger = new LoggerConfiguration()
                        .MinimumLevel.Information()
                        .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                        .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}", theme: AnsiConsoleTheme.Code)
                        .CreateLogger();

                    x.AddSerilog(logger, true);

                    x.Services.Remove(x.Services.First(y => y.ServiceType == typeof(ILogger<>)));
                    x.Services.AddSingleton(typeof(ILogger<>), typeof(Logger<>));
                })
                .ConfigureHostConfiguration(configuration => configuration
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile(Global.ConfigPath))
                .ConfigureServices(((_, services) =>
                {
                    services.AddDbContextFactory<DatabaseContext>();
                    services.AddSingleton<TypeLoaderService>();
                    services.AddSingleton<SearchService>();
                    services.AddSingleton<TagService>();
                }))
                .ConfigureDiscordBot((context, bot) =>
                {
                    bot.Token = context.Configuration["discord:token"];
                    bot.Intents = GatewayIntents.Recommended;
                    bot.Prefixes = context.Configuration.GetSection("discord:prefixes").Get<string[]>();
                })
                .Build();

            using (var scope = host.Services.CreateScope())
            {
                var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
                var context = scope.ServiceProvider.GetRequiredService<IDbContextFactory<DatabaseContext>>().CreateDbContext();
                logger.LogInformation("Migrating database....");
                await context.Database.MigrateAsync();
                logger.LogInformation("Done migrating database");
            }

            using (host)
            {
                await host.RunAsync();
            }
        }
    }
}
