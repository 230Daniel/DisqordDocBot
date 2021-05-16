using System;
using System.Linq;
using System.Reflection;
using Disqord;
using DisqordDocBot.Extensions;
using DisqordDocBot.Services;

namespace DisqordDocBot.Search
{
    public abstract class SearchableMember : ISearchable
    {
        public string Summary { get; }
        public SearchableType Parent { get; }
        public abstract MemberInfo Info { get; }


        protected SearchableMember(SearchableType parent, string summary)
        {
            Parent = parent;
            Summary = summary;
        }

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
                .WithDescription(Summary);

            return eb;
        }

        public override string ToString() 
            => Info.DeclaringType is null ? Info.Name : $"{Parent.Info.Humanize()}.{Info.Name.Split('.').Last()}";

        public static SearchableMember Create(MemberInfo info, SearchableType parent, DocumentationLoaderService documentationLoaderService)
        {
            var summary = documentationLoaderService.GetSummaryFromInfo(info);
            return info switch
            {
                PropertyInfo propertyInfo => new SearchableProperty(propertyInfo, parent, summary),
                MethodInfo {DeclaringType: { }} methodInfo when methodInfo.DeclaringType == typeof(object) => new HiddenSearchable(methodInfo, parent, summary),
                MethodInfo methodInfo when methodInfo.IsExtensionMethod() => new SearchableExtensionMethod(methodInfo, parent, summary),
                MethodInfo methodInfo => new SearchableMethod(methodInfo, parent, summary),
                FieldInfo fieldInfo => new SearchableField(fieldInfo, parent, summary),
                EventInfo eventInfo => new SearchableEvent(eventInfo, parent, summary),
                ConstructorInfo constructorInfo => new SearchableConstructor(constructorInfo, parent, summary),
                TypeInfo typeInfo => new HiddenSearchable(typeInfo, parent, summary),
                _ => new SearchableUnknown(info, parent, summary)
            };
        }
    }
}