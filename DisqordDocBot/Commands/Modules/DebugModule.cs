using Disqord.Bot;
using Qmmands;

namespace DisqordDocBot.Commands.Modules
{
    public class DebugModule : DiscordGuildModuleBase
    {
        [Command("help")]
        public DiscordCommandResult Help()
            => Response("Go fuck yourself");
    }
}