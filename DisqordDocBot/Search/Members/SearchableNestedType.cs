using System.Reflection;

namespace DisqordDocBot.Search
{
    // TODO: Figure out nested types
    public class SearchableNestedType : SearchableMember
    {
        public SearchableNestedType(MemberInfo info, SearchableType parent) : base(info, parent) { }
    }
}