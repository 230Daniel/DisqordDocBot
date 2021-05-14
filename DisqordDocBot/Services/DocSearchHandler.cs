using System.Threading.Tasks;
using Disqord;
using Disqord.Bot;
using Disqord.Gateway;
using Disqord.Hosting;
using Disqord.Rest;
using Microsoft.Extensions.Logging;

namespace DisqordDocBot.Services
{
    public class DocSearchService : DiscordClientService
    {
        private readonly SearchService _searchService;
        private MentionPrefix _prefix;

        public DocSearchService(SearchService searchService, ILogger<DocSearchService> logger, DiscordClientBase client) : base(logger, client)
        {
            _searchService = searchService;
            client.MessageReceived += HandleDocSearch;
            
        }

        protected override ValueTask OnReady(ReadyEventArgs e)
        {
            _prefix = new MentionPrefix(Client.CurrentUser.Id);
            return ValueTask.CompletedTask;
        }

        private async ValueTask HandleDocSearch(object sender, MessageReceivedEventArgs e)
        {
            if (e.Member.IsBot)
                return;

            if (e.Message is not IGatewayUserMessage userMessage)
                return;

            
            if (_prefix.TryFind(userMessage, out var queryString))
            {
                var result = _searchService.GetMostRelevantItem(queryString.Trim());

                if (result is null)
                    await Client.SendMessageAsync(userMessage.ChannelId, new LocalMessageBuilder().WithContent("No results found").Build());
                else
                {
                    await Client.SendMessageAsync(userMessage.ChannelId, new LocalMessageBuilder().WithEmbed(result.CreateInfoEmbed()).Build());
                }
                
                    
            }
 
        }
    }
}