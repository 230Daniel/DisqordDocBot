using System.Threading.Tasks;
using Disqord.Bot;
using DisqordDocBot.Services;
using Qmmands;

namespace DisqordDocBot.Commands.Modules
{
    [Group("tag")]
    public class TagModule : DiscordGuildModuleBase
    {
        private readonly TagService _tagService;
        
        public TagModule(TagService tagService)
        {
            _tagService = tagService;
        }
        
        [Command("")]
        public async Task Tag([Remainder] string name)
        {
            var tag = await _tagService.GetTestTagAsync(name);
            if (tag is null) await Response("tag not found");
            else await Response(tag.Value);
        }
        
        [Command("create")]
        public async Task Create(string name, [Remainder] string value)
        {
            await _tagService.SaveTestTagAsync(name, value);
            await Response("tag created");
        }
    }
}