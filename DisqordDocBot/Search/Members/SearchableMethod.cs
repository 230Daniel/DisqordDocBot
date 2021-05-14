using System.Reflection;
using Disqord;
using DisqordDocBot.Extensions;

namespace DisqordDocBot.Search
{
    public class SearchableMethod : SearchableMember
    {
        public override MethodInfo Info { get; }

        public SearchableMethod(MethodInfo info, SearchableType parent) : base(info, parent)
        {
            Info = info;
        }

        public override LocalEmbedBuilder CreateInfoEmbed() 
            => base.CreateInfoEmbed()
                .AddField("Arguments", Markdown.Code(Info.CreateArgString()))
                .AddField("Return Type", Markdown.Code(Info.ReturnType.Humanize()));

        public override string ToString() 
            => $"Method: {base.ToString()}";

    }
}