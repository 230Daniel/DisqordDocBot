using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Disqord;
using DisqordDocBot.Extensions;

namespace DisqordDocBot.Search
{
    public class SearchableType : ISearchable
    {
        public TypeInfo Info { get; }
        
        public IReadOnlyList<SearchableMember> Members { get; }
        
        public SearchableType(TypeInfo info)
        {
            Info = info;
            Members = info.GetAllMembers().Select(x => SearchableMember.Create(x, this)).ToList();
        }

        public int Priority => Global.TypePriority;

        public RelevanceScore GetRelevanceScore(string query)
        {
            if (string.Equals(query, Info.Name, StringComparison.OrdinalIgnoreCase))
                return RelevanceScore.FullMatch;
            else if(Info.Name.Contains(query, StringComparison.OrdinalIgnoreCase))
                return RelevanceScore.PartialMatch;
            else
                return RelevanceScore.NoMatch;
        }

        public SearchableMember SearchMembers(string query)
        {
            if (!Members.Any())
                    return null;
            
            var topResult = Members.SortByRelevance(query).FirstOrDefault();

            if (topResult.Value == RelevanceScore.NoMatch)
                return null;

            return topResult.Key;
        }

        public LocalEmbedBuilder CreateInfoEmbed()
        {
            var eb = new LocalEmbedBuilder()
                .WithDefaultColor()
                .WithTitle(Info.FullName)
                .WithDescription("docs here");

            var definedProperties = Info.GetProperties();
            var definedMethods = Info.GetMethods();
            var displayMethods = new List<string>(3);
            foreach (var method in definedMethods)
            {
                if (displayMethods.Count == 3)
                    break;
                
                if (definedProperties.Any(x => x.GetMethod == method || x.SetMethod == method))
                    continue;
                
                displayMethods.Add(method.Name);
            }
            
            
            if(definedProperties.Length > 0)
                eb.AddField("Properties", string.Join('\n', definedProperties.Take(3).Select(x => x.Name)), true);

            eb.AddBlankField(true);
            
            if(displayMethods.Count > 0)
                eb.AddField("Methods", string.Join('\n', displayMethods), true);
            

            return eb;
        }
    }
}