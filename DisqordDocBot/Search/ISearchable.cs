using Disqord;

namespace DisqordDocBot.Search
{
    public interface ISearchable
    {
        public string Summary { get; }
        public RelevanceScore GetRelevanceScore(string query);
        public LocalEmbedBuilder CreateInfoEmbed();
    }
}