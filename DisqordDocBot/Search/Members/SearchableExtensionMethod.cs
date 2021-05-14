using System.Reflection;
using DisqordDocBot.Extensions;

namespace DisqordDocBot.Search
{
    public class SearchableExtensionMethod : SearchableMethod
    {
        public SearchableExtensionMethod(MethodInfo info, SearchableType parent) : base(info, parent) { }

        protected override string CreateArgString()
            => $"(this {Info.CreateArgString()})";

        public override string ToString() 
        => $"Extension {base.ToString()}";
    }
}