using System.Reflection;

namespace DisqordDocBot.Search
{
    public class SearchableUnknown : SearchableMember
    {
        public override MemberInfo Info { get; }

        public SearchableUnknown(MemberInfo info, SearchableType parent)
            : base(parent)
        {
            Info = info;
        }

        public override string ToString() 
            => $"Unknown: {base.ToString()}";
    }
}