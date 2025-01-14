using Disqord;

namespace DisqordDocBot
{
    public class Global
    {
        public const string ConfigPath = "./config.json";
        public static readonly Color DefaultEmbedColor = new(0x2F3136);

        public static readonly Snowflake AuthorId = 332675511161585666;
        public static readonly Snowflake[] ContributorIds = { 608143610415939638, 218613903653863427 };

        public const int NamespacePriority = 2;
        public const int TypePriority = 1;
        public const int TypeMemberPriority = 0;

        public const string DisqordNamespace = "Disqord";
        public const char GenericNameCharacter = '`';

        // these are here just to make sure types from all projects get discovered
#pragma warning disable 169
        private static Disqord.Bot.DiscordBot _discordBot;
        private static Snowflake _disqordCore;
        private static Disqord.Gateway.Api.IGateway _disqordGatewayApi;
        private static Disqord.Gateway.CachedChannel _disqordGateway;
        private static Disqord.Rest.Api.Route _disqordRestApi;
        private static Disqord.Rest.IRestClient _disqordRest;
        private static Disqord.Webhook.IWebhookClient _disqordWebhook;
#pragma warning restore 169
    }
}
