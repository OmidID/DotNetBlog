using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System;

namespace DotNetBlog.Data.Factory
{
    class NpgsqlFactory : IDesignTimeDbContextFactory<PostgreeSqlBlogContext>
    {
        private readonly IConfigurationRoot _configuration;

        public NpgsqlFactory()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            _configuration = builder.Build();
        }

        public PostgreeSqlBlogContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<BlogContext>();
            optionsBuilder.UseNpgsql(_configuration["ConnectionStrings:PostgreSQL"]);

            return new PostgreeSqlBlogContext(optionsBuilder.Options, null);
        }
    }
}
