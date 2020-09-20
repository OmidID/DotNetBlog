using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System;

namespace DotNetBlog.Core.Data.Factory
{
    public class SqlServerFactory : IDesignTimeDbContextFactory<SqlServerBlogContext>
    {
        private readonly IConfigurationRoot _configuration;

        public SqlServerFactory()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            _configuration = builder.Build();
        }

        public SqlServerBlogContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<BlogContext>();
            optionsBuilder.UseSqlServer(_configuration["ConnectionStrings:MSSQL"]);

            return new SqlServerBlogContext(optionsBuilder.Options, null);
        }
    }
}
