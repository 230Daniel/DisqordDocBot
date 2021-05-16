using System.Reflection;
using DisqordDocBot.Extensions;

namespace DisqordDocBot.Search
{
    public class SearchableExtensionMethod : SearchableMethod
    {
        public SearchableExtensionMethod(MethodInfo info, SearchableType parent, string summary) 
            : base(info, parent, summary) { }

        protected override string CreateArgString()
            => $"({Info.CreateArgString()})";

        public override string ToString() 
        => $"Extension {base.ToString()}";
    }
}