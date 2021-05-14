using System.Reflection;

namespace DisqordDocBot.Search
{
    public class HiddenSearchable : SearchableMember
    {
        public HiddenSearchable(MemberInfo info, SearchableType parent) : base(info, parent) { }
        
    }
}