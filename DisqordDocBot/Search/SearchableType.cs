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

            if (Info.Name.Contains(query, StringComparison.OrdinalIgnoreCase))
                return RelevanceScore.PartialMatch;

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

        public LocalEmbed CreateInfoEmbed()
        {
            var eb = new LocalEmbed()
                .WithDefaultColor()
                .WithTitle(ToString())
                .WithDescription(Summary);

            var displayMethods = new List<string>();
            var displayProperties = new List<string>();
            var propertyCount = 0;
            var methodCount = 0;
            foreach (var member in Members)
            {
                if (member is SearchableMethod method)
                {
                    if (displayMethods.Count < 3 && !displayMethods.Contains(method.Info.Name))
                        displayMethods.Add(method.Info.Name);
                    methodCount++;
                }
                else if (member is SearchableProperty property)
                {
                    if (displayProperties.Count < 3 && !displayProperties.Contains(property.Info.Name))
                        displayProperties.Add(property.Info.Name);
                    propertyCount++;
                }
            }

            if (displayProperties.Count > 0)
            {
                eb.AddInlineCodeBlockField($"Properties ({propertyCount})", string.Join("\n", displayProperties));
                eb.AddInlineBlankField();
            }

            if (displayMethods.Count > 0)
                eb.AddInlineCodeBlockField($"Methods ({methodCount})", string.Join('\n', displayMethods));

            return eb;
        }

        public override string ToString()
            => Info.IsEnum ? $"Enum: {Info.FullName}" : $"Type: {Info.Humanize()}";
    }
}
