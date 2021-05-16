using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Disqord;

namespace DisqordDocBot.Tags
{
    public class Tag
    {
        public Snowflake GuildId { get; set; }
        public string Name { get; set; }
        public Snowflake MemberId { get; set; }
        public string Content { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public uint Uses { get; set; }
    }
}