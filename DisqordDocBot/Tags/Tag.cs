using System.ComponentModel.DataAnnotations;
using Disqord;

namespace DisqordDocBot.Tags
{
    public class Tag
    {
        public Snowflake GuildId { get; set; }
        public string Name { get; set; }
        public Snowflake MemberId { get; set; }
        public string Value { get; set; }
    }
}