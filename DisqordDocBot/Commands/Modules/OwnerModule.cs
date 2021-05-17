using System;
using Disqord.Bot;
using Qmmands;

namespace DisqordDocBot.Commands.Modules
{
    [RequireBotOwner]
    public class OwnerModule : DiscordGuildModuleBase
    {
        [Command("shutdown", "stop", "die", "kill", "exit")]
        public void Shutdown()
            => Environment.Exit(0);
        
        
        [Command("restart", "update")]
        public void Restart()
            => Environment.Exit(1);
    }
}