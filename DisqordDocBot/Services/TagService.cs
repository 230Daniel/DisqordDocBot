using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Disqord;
using DisqordDocBot.EFCore;
using DisqordDocBot.Tags;
using Microsoft.EntityFrameworkCore;
using MinimumEditDistance;

namespace DisqordDocBot.Services
{
    public class TagService
    {
        private readonly IDbContextFactory<DatabaseContext> _dbContextFactory;

        public TagService(IDbContextFactory<DatabaseContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }

        public async Task<Tag> GetTagAsync(Snowflake guildId, string name)
        {
            await using var db = _dbContextFactory.CreateDbContext();
            return await db.Tags.FirstOrDefaultAsync(x => x.GuildId == guildId && EF.Functions.ILike(x.Name, name));
        }

        public async Task<List<Tag>> GetTagsAsync(Snowflake guildId)
        {
            await using var db = _dbContextFactory.CreateDbContext();
            return await db.Tags.Where(x => x.GuildId == guildId).ToListAsync();
        }

        public async Task<List<Tag>> SearchTagsAsync(Snowflake guildId, string query)
        {
            var tags = await GetTagsAsync(guildId);
            return tags
                .Select(x => (Levenshtein.CalculateDistance(x.Name, query, 2), x))
                .Where(x => x.Item1 <= 5)
                .OrderBy(x => x.Item1)
                .Select(x => x.Item2)
                .ToList();
        }

        public async Task<int> GetTagRankAsync(Tag tag)
        {
            var tags = await GetTagsAsync(tag.GuildId);
            return tags.OrderByDescending(x => x.Uses).TakeWhile(x => x.Name != tag.Name).Count() + 1;
        }

        public async Task CreateTagAsync(Tag tag)
        {
            await using var db = _dbContextFactory.CreateDbContext();
            await db.Tags.AddAsync(tag);
            await db.SaveChangesAsync();
        }

        public async Task UpdateTagAsync(Tag tag)
        {
            await using var db = _dbContextFactory.CreateDbContext();
            db.Tags.Update(tag);
            await db.SaveChangesAsync();
        }

        public async Task RemoveTagAsync(Tag tag)
        {
            await using var db = _dbContextFactory.CreateDbContext();
            db.Tags.Attach(tag);
            db.Tags.Remove(tag);
            await db.SaveChangesAsync();
        }
    }
}
