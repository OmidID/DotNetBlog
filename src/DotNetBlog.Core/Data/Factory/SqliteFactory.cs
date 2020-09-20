using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System;

namespace DotNetBlog.Core.Data.Factory
{
    public class SqliteFactory : IDesignTimeDbContextFactory<SqliteBlogContext>
    {
        private readonly IConfigurationRoot _configuration;

        public SqliteFactory()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            _configuration = builder.Build();
        }

        public SqliteBlogContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<BlogContext>();
            optionsBuilder.UseSqlite(_configuration["ConnectionStrings:SQLite"]);

            return new SqliteBlogContext(optionsBuilder.Options, null);
        }
    }
}
