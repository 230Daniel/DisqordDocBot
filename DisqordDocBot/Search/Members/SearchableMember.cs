using System;
using System.Reflection;
using Disqord;
using DisqordDocBot.Extensions;

namespace DisqordDocBot.Search
{
    public abstract class SearchableMember : ISearchable
    {
        public SearchableType Parent { get; }
        public virtual MemberInfo Info { get; }

        protected SearchableMember(MemberInfo info, SearchableType parent)
        {
            Info = info;
            Parent = parent;
        }

        public int Priority => Global.TypeMemberPriority;

        public virtual RelevanceScore GetRelevanceScore(string query)
        {
            var comparisonString = Info.Name;
            
            if (string.Equals(comparisonString, query, StringComparison.OrdinalIgnoreCase))
                return RelevanceScore.FullMatch;
            else if (comparisonString.Contains(query, StringComparison.OrdinalIgnoreCase))
                return RelevanceScore.PartialMatch;
            else
                return RelevanceScore.NoMatch;
        }

        public virtual LocalEmbedBuilder CreateInfoEmbed()
        {
            var eb = new LocalEmbedBuilder()
                .WithDefaultColor()
                .WithTitle(ToString())
                .WithDescription("DOCS HERE IDIOT");

            return eb;
        }

        public override string ToString() 
            => Info.DeclaringType is null ? Info.Name : $"{Parent.Info.Name}.{Info.Name}";

        public static SearchableMember Create(MemberInfo info, SearchableType parent)
        {
            return info switch
            {
                PropertyInfo propertyInfo => new SearchableProperty(propertyInfo, parent),
                MethodInfo methodInfo => new SearchableMethod(methodInfo, parent),
                FieldInfo fieldInfo => new SearchableField(fieldInfo, parent),
                EventInfo eventInfo => new SearchableEvent(eventInfo, parent),
                ConstructorInfo constructorInfo => new SearchableConstructor(constructorInfo, parent),
                _ => new SearchableUnknown(info, parent)
            };
        }
    }
}