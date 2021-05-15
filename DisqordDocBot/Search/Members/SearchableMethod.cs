using System.Reflection;
using Disqord;
using DisqordDocBot.Extensions;

namespace DisqordDocBot.Search
{
    public class SearchableMethod : SearchableMember
    {
        public override MethodInfo Info { get; }

        public SearchableMethod(MethodInfo info, SearchableType parent) : base(parent)
        {
            Info = info;
        }

        public override LocalEmbedBuilder CreateInfoEmbed()
        {
            var eb = base.CreateInfoEmbed()
                .AddCodeBlockField("Arguments", CreateArgString())
                .AddCodeBlockField("Return Type", Info.ReturnType.Humanize());

            return eb;
        }

        protected virtual string CreateArgString()
            => $"({Info.CreateArgString()})";

        public override string ToString() 
            => $"Method: {base.ToString()}";

    }
}