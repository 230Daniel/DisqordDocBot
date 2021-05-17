using System;
using Disqord.Bot;
using Microsoft.Extensions.Hosting;
using Qmmands;

namespace DisqordDocBot.Commands.Modules
{
    [RequireBotOwner]
    public class OwnerModule : DiscordGuildModuleBase
    {
        private readonly IHostApplicationLifetime _lifetime;

        public OwnerModule(IHostApplicationLifetime lifetime)
        {
            _lifetime = lifetime;
        }
        
        [Command("shutdown", "stop", "die", "kill", "exit")]
        public void Shutdown()
            => _lifetime.StopApplication();
        
        
        [Command("restart", "update")]
        public void Restart()
        {
            Environment.ExitCode = 1;
            _lifetime.StopApplication();
        }
    }
}