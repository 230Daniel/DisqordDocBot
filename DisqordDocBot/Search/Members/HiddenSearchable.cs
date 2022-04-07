using System.Reflection;

namespace DisqordDocBot.Search
{
    public class HiddenSearchable : SearchableMember
    {
        public override MemberInfo Info { get; }

        public HiddenSearchable(MemberInfo info, SearchableType parent, string summary)
            : base(parent, summary)
        {
            Info = info;
        }
    }
}
