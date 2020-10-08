using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System;

namespace DotNetBlog.Data.Factory
{
    public class MySqlFactory : IDesignTimeDbContextFactory<MySqlBlogContext>
    {
        private readonly IConfigurationRoot _configuration;

        public MySqlFactory()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            _configuration = builder.Build();
        }

        public MySqlBlogContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<BlogContext>();
            optionsBuilder.UseMySQL(_configuration["ConnectionStrings:MySQL"]);

            return new MySqlBlogContext(optionsBuilder.Options, null);
        }
    }
}
