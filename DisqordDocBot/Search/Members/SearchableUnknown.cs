using System.Reflection;

namespace DisqordDocBot.Search
{
    public class SearchableUnknown : SearchableMember
    {
        public SearchableUnknown(MemberInfo info, SearchableType parent)
            : base(info, parent)
        {
           
        }
        
        public override string ToString() 
            => $"Unknown: {base.ToString()}";
    }
}