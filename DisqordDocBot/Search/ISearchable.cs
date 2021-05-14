using Disqord;

namespace DisqordDocBot.Search
{
    public interface ISearchable
    {
        // TODO: Documentation property
        public int Priority { get; }

        public RelevanceScore GetRelevanceScore(string query);
        public LocalEmbedBuilder CreateInfoEmbed();
    }
}