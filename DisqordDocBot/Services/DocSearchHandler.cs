using System.Threading.Tasks;
using Disqord;
using Disqord.Bot;
using Disqord.Bot.Hosting;
using Disqord.Rest;
using Microsoft.Extensions.Logging;

namespace DisqordDocBot.Services
{
    public class DocSearchService : DiscordBotService
    {
        private readonly SearchService _searchService;

        public DocSearchService(SearchService searchService, ILogger<DocSearchService> logger, DiscordBotBase client) : base(logger, client)
        {
            _searchService = searchService;
        }

        protected override async ValueTask OnCommandNotFound(DiscordCommandContext context)
        {
            var result = _searchService.GetMostRelevantItem(context.Input.Trim());

            if (result is null)
                await Client.SendMessageAsync(context.ChannelId, new LocalMessage().WithContent("No results found"));
            else
                await Client.SendMessageAsync(context.ChannelId, new LocalMessage().AddEmbed(result.CreateInfoEmbed()));
        }
    }
}
