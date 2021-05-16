using System;
using System.Threading.Tasks;
using DisqordDocBot.EFCore;
using DisqordDocBot.Tags;
using Microsoft.EntityFrameworkCore;

namespace DisqordDocBot.Services
{
    public class TagService
    {
        private readonly IDbContextFactory<DatabaseContext> _dbContextFactory;
        
        public TagService(IDbContextFactory<DatabaseContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }

        public async Task<Tag> GetTestTagAsync(string name)
        {
            await using var db = _dbContextFactory.CreateDbContext();
            return await db.Tags.FirstOrDefaultAsync(x => x.Name == name);
        }
        
        public async Task SaveTestTagAsync(string name, string value)
        {
            await using var db = _dbContextFactory.CreateDbContext();
            var tag = new Tag
            {
                GuildId = 0,
                Name = name,
                MemberId = 0,
                Value = value
            };
            await db.Tags.AddAsync(tag);
            await db.SaveChangesAsync();
        }
    }
}