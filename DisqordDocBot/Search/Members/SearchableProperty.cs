using System.Reflection;
using Disqord;
using DisqordDocBot.Extensions;

namespace DisqordDocBot.Search
{
    public class SearchableProperty : SearchableMember
    {
        public override PropertyInfo Info { get; }

        public SearchableProperty(PropertyInfo info, SearchableType parent) : base(info, parent)
        {
            Info = info;
        }
        
        public override string ToString() 
            => $"Property: {base.ToString()}";

        public override LocalEmbedBuilder CreateInfoEmbed() 
            => base.CreateInfoEmbed().AddField("Type", Info.PropertyType.Humanize());
    }
}