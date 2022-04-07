using System;
using System.Threading.Tasks;
using Disqord.Bot;
using Disqord.Gateway;
using Disqord.Rest;
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
        public async Task Shutdown()
        {
            await Response("Shutting down");
            _lifetime.StopApplication();
        }

        [Command("restart", "update")]
        public async Task Restart()
        {
            await Response("Restarting");
            Environment.ExitCode = 1;
            _lifetime.StopApplication();
        }

        [Command("nickname", "nick")]
        public async Task Nickname([Remainder] string nickname)
        {
            await Context.Guild.GetMember(Context.Bot.CurrentUser.Id).ModifyAsync(x => x.Nick = nickname);
            await Response("Changed nickname");
        }
    }
}
