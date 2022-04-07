using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Disqord;
using Disqord.Bot;
using Disqord.Rest;
using DisqordDocBot.Extensions;
using Qmmands;

namespace DisqordDocBot.Commands.Modules
{
    public class InfoModule : DiscordGuildModuleBase
    {
        [Command("info")]
        public DiscordCommandResult Info()
        {
            var eb = new LocalEmbed()
                .WithDefaultColor()
                .WithTitle("Disqord Doc Bot")
                .WithDescription(
                    $"Written by: {Mention.User(Global.AuthorId)}\n\nWith contributions from:\n{string.Join('\n', Global.ContributorIds.Select(x => Mention.User(x)))}");

            return Response(eb);
        }

        [Command("ping")]
        public async Task Ping()
        {
            var sw = Stopwatch.StartNew();
            var msg = await Response("Latency: *loading*");
            sw.Stop();

            await msg.ModifyAsync(x => x.Content = $"Latency: {(int)sw.ElapsedMilliseconds}ms");
        }
    }
}
