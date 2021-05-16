using Disqord;

namespace DisqordDocBot
{
    public class Global
    {
        public static readonly Color DefaultEmbedColor = new(0x2F3136);
        public const int NamespacePriority = 2;
        public const int TypePriority = 1;
        public const int TypeMemberPriority = 0;
        
        public const string DisqordNamespace = "Disqord";

        // these are here just to make sure types from all projects get discovered
        private static Disqord.Bot.DiscordBot _discordBot;
        private static Disqord.Snowflake _disqordCore;
        private static Disqord.Gateway.Api.IGateway _disqordGatewayApi;
        private static Disqord.Gateway.CachedChannel _disqordGateway;
        private static Disqord.Rest.Api.Route _disqordRestApi;
        private static Disqord.Rest.IRestClient _disqordRest;
        private static Disqord.Webhook.IWebhookClient _disqordWebhook;
    }
}