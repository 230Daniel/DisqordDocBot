using System.IO;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace DisqordDocBot.EFCore
{
    public class DesignTimeDatabaseContextFactory : IDesignTimeDbContextFactory<DatabaseContext> 
    {
        public DatabaseContext CreateDbContext(string[] args) 
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile(Global.ConfigPath)
                .Build();

            return new DatabaseContext(configuration);
        } 
    }
}