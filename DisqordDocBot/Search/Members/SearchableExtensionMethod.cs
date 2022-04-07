using System.Reflection;

namespace DisqordDocBot.Search
{
    public class SearchableExtensionMethod : SearchableMethod
    {
        public SearchableExtensionMethod(MethodInfo info, SearchableType parent, string summary)
            : base(info, parent, summary)
        {
        }

        public override string ToString()
            => $"Extension {base.ToString()}";
    }
}
