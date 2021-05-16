using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Disqord.Bot;
using Disqord.Bot.Hosting;
using DisqordDocBot.Extensions;
using DisqordDocBot.Search;
using Microsoft.Extensions.Logging;

namespace DisqordDocBot.Services
{
    public class SearchService : DiscordBotService
    {
        private const string ScopeSplittingString = ".";
        private readonly TypeLoaderService _typeLoaderService;
        private readonly DocumentationLoaderService _documentationLoaderService;
        private readonly List<ISearchable> _allSearchables;
        private readonly List<SearchableType> _searchableTypes;
        
        public SearchService(TypeLoaderService typeLoaderService, DocumentationLoaderService documentationLoaderService, ILogger<SearchService> logger, DiscordBotBase client) : base(logger, client)
        {
            _typeLoaderService = typeLoaderService;
            _documentationLoaderService = documentationLoaderService;
            _allSearchables = new List<ISearchable>();
            _searchableTypes = new List<SearchableType>();
            // BuildCaches();
        }

        public ISearchable GetMostRelevantItem(string query)
        {
            var scopes = query.Trim().Split(ScopeSplittingString);

            return (scopes.Length - 1) switch
            {
                Global.TypeMemberPriority => GlobalSearch(scopes[0]),
                Global.TypePriority => TypeMemberSearch(scopes),
                _ => null
            };
        }

        private ISearchable GlobalSearch(string query)
        {
            var sw = Stopwatch.StartNew();
            var topResult = _allSearchables.SortByRelevance(query).FirstOrDefault();

            Logger.LogInformation($"Completed global search in {sw.ElapsedMilliseconds}ms");
            
            if (topResult.Value == RelevanceScore.NoMatch)
                return null;

            return topResult.Key;
        }
        
        private ISearchable TypeMemberSearch(string[] scopes)
        {
            var sw = Stopwatch.StartNew();

            foreach (var result in _searchableTypes.SortByRelevance(scopes[0]))
            {
                if (result.Value == RelevanceScore.NoMatch)
                {
                    sw.Stop();
                    Logger.LogInformation($"Completed type member search in {sw.ElapsedMilliseconds}ms");
                    return null;
                }

                var memberResult = result.Key.SearchMembers(scopes[1]);

                if (memberResult is not null)
                {
                    sw.Stop();
                    Logger.LogInformation($"Completed type member search in {sw.ElapsedMilliseconds}ms");
                    return memberResult;
                }
            }

            sw.Stop();
            Logger.LogInformation($"Completed type member search in {sw.ElapsedMilliseconds}ms");
            return null;
        }

        private void BuildCaches()
        {
            var sw = Stopwatch.StartNew();
            var extensionMethods = _typeLoaderService.GetAllExtensionMethods();
            foreach (var type in _typeLoaderService.LoadedTypes)
            {
                var summary = "";
                if (_documentationLoaderService.Documentation.TryGetValue($"T:{type.FullName}", out var sum))
                    summary = sum;
                // var summary = _documentationLoaderService.Documentation[$"T:{type.FullName}"];
                var searchableType = new SearchableType(type, extensionMethods.Where(x => type.IsAssignableTo(x.GetParameters().First().ParameterType)), summary);
                _allSearchables.AddRange(searchableType.Members.Where(x => x.Info.DeclaringType == type));
                
                _allSearchables.Add(searchableType);
                _searchableTypes.Add(searchableType);
            }
            sw.Stop();
            Logger.LogInformation($"Built search caches in {sw.ElapsedMilliseconds}ms");
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            BuildCaches();
            return base.StartAsync(cancellationToken);
        }
    }
}