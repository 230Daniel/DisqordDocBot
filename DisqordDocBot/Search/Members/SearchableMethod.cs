using System.Reflection;
using Disqord;
using DisqordDocBot.Extensions;

namespace DisqordDocBot.Search
{
    public class SearchableMethod : SearchableMember
    {
        public override MethodInfo Info { get; }

        public SearchableMethod(MethodInfo info, SearchableType parent, string summary)
            : base(parent, summary)
        {
            Info = info;
        }

        public override LocalEmbed CreateInfoEmbed()
        {
            var eb = base.CreateInfoEmbed()
                .AddCodeBlockField("Arguments", CreateArgString())
                .AddCodeBlockField("Return Type", Info.ReturnType.Humanize());

            return eb;
        }

        protected virtual string CreateArgString()
            => $"{Info.CreateGenericArgString()}({Info.CreateArgString()})";

        public override string ToString()
            => $"Method: {base.ToString()}";

    }
}
