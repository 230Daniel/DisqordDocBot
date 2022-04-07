using Disqord;
using DisqordDocBot.Extensions;
using DisqordDocBot.Tags;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.Configuration;

namespace DisqordDocBot.EFCore
{
    public class DatabaseContext : DbContext
    {
        public DbSet<Tag> Tags { get; set; }

        private readonly string _connectionString;

        public DatabaseContext(IConfiguration config)
        {
            _connectionString = config["database:connection"];
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options
                .UseNpgsql(_connectionString)
                .UseSnakeCaseNamingConvention();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.UseValueConverterForType<Snowflake>(new ValueConverter<Snowflake, ulong>(x => x.RawValue, x => new Snowflake(x)));
            modelBuilder.Entity<Tag>().HasKey("GuildId", "Name");
        }
    }
}
