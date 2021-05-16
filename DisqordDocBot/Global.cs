using Disqord;

namespace DisqordDocBot
{
    public class Global
    {
        public const string ConfigPath = "./config.json";
        public static readonly Color DefaultEmbedColor = new(0x2F3136);
        public const int NamespacePriority = 2;
        public const int TypePriority = 1;
        public const int TypeMemberPriority = 0;
    }
}