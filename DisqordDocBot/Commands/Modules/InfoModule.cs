using System.Linq;
using Disqord;
using Disqord.Bot;
using DisqordDocBot.Extensions;
using Qmmands;

namespace DisqordDocBot.Commands.Modules
{
    public class InfoModule : DiscordGuildModuleBase
    {
        [Command("info")]
        public DiscordCommandResult Info()
        {
            var eb = new LocalEmbedBuilder()
                .WithDefaultColor()
                .WithTitle("Disqord Doc Bot")
                .WithDescription(
                    $"Written by: {Mention.User(Global.AuthorId)}\n\nWith contributions from:\n{string.Join('\n', Global.ContributorIds.Select(x => Mention.User(x)))}");

            return Response(eb);
        }
    }
}