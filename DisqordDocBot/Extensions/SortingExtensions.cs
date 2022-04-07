using System.Collections.Generic;
using System.Linq;
using DisqordDocBot.Search;

namespace DisqordDocBot.Extensions
{
    public static class SortingExtensions
    {
        public static IOrderedEnumerable<KeyValuePair<T, RelevanceScore>> SortByRelevance<T>(this IReadOnlyList<T> searchables, string query)
            where T : ISearchable
        {
            var resultDict = new Dictionary<T, RelevanceScore>(searchables.Count);

            foreach (var searchableType in searchables)
            {
                if (searchableType is HiddenSearchable)
                    continue;
                resultDict.Add(searchableType, searchableType.GetRelevanceScore(query));
            }

            return resultDict.OrderByDescending(x => x.Value);
        }
    }
}
