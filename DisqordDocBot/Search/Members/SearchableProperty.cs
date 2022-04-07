using System.Reflection;
using System.Text;
using Disqord;
using DisqordDocBot.Extensions;

namespace DisqordDocBot.Search
{
    public class SearchableProperty : SearchableMember
    {
        public override PropertyInfo Info { get; }

        public SearchableProperty(PropertyInfo info, SearchableType parent, string summary)
            : base(parent, summary)
        {
            Info = info;
        }

        public override string ToString()
            => $"Property: {base.ToString()}";

        public override LocalEmbed CreateInfoEmbed() =>
            base.CreateInfoEmbed()
                .AddCodeBlockField("Accessors", BuildAccessorString())
                .AddCodeBlockField("Type", Info.PropertyType.Humanize());

        private string BuildAccessorString()
        {
            var sb = new StringBuilder("{ ");

            if (Info.CanRead)
                sb.Append("get; ");

            if (Info.CanWrite)
                sb.Append("set; ");

            return sb.Append('}').ToString();
        }
    }
}
