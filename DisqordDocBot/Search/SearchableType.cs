using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Disqord;
using DisqordDocBot.Extensions;
using DisqordDocBot.Services;

namespace DisqordDocBot.Search
{
    public class SearchableType : ISearchable
    {
        public string Summary { get; }
        
        public TypeInfo Info { get; }
        
        public IReadOnlyList<SearchableMember> Members { get; }
        
        public SearchableType(TypeInfo info, TypeLoaderService typeLoaderService, DocumentationLoaderService documentationLoaderService)
        {
            Info = info;
            Members = info.GetAllMembers().Select(x => SearchableMember.Create(x, this, documentationLoaderService))
                .Concat(typeLoaderService.ExtensionMethods
                    .Where(x => info.IsAssignableTo(x.GetParameters().First().ParameterType))
                    .Select(x => SearchableMember.Create(x, this, documentationLoaderService)))
                .ToList();

            Summary = documentationLoaderService.GetSummaryFromInfo(info);
        }

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
                .WithTitle(ToString())
                .WithDescription(Summary);

            var displayMethods = new List<string>();
            var displayProperties = new List<string>();
            foreach (var member in Members)
            {
                if (displayMethods.Count == 3 && displayProperties.Count == 3)
                    break;

                if (displayMethods.Count < 3 && member is SearchableMethod method && !displayMethods.Contains(method.Info.Name))
                    displayMethods.Add(method.Info.Name);
                else if (displayProperties.Count < 3 && member is SearchableProperty property && !displayProperties.Contains(property.Info.Name))
                    displayProperties.Add(property.Info.Name);
            }
            
            if (displayProperties.Count > 0)
            {
                eb.AddInlineCodeBlockField("Properties", string.Join("\n", displayProperties));
                eb.AddInlineBlankField();
            }

            if (displayMethods.Count > 0)
                eb.AddInlineCodeBlockField("Methods", string.Join('\n', displayMethods));
            
            return eb;
        }

        public override string ToString()
            => Info.IsEnum ? $"Enum: {Info.FullName}" : $"Type: {Info.FullName}";
    }
}