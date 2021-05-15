using System.Reflection;
using Disqord;
using DisqordDocBot.Extensions;

namespace DisqordDocBot.Search
{
    public class SearchableConstructor : SearchableMember
    {
        public override ConstructorInfo Info { get; }

        public SearchableConstructor(ConstructorInfo info, SearchableType parent)
            : base(parent)
        {
            Info = info;
        }        
        public override LocalEmbedBuilder CreateInfoEmbed() 
            => base.CreateInfoEmbed().AddCodeBlockField("Arguments", Info.CreateArgString());
        
        public override string ToString() 
            => $"Constructor: {Info.DeclaringType!.Name}";
    }
}