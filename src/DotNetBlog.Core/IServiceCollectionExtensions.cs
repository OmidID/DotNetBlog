using DotNetBlog.Data;
using DotNetBlog.Service;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Reflection;

namespace DotNetBlog
{
    public static class IServiceCollectionExtensions
    {
        public static IServiceCollection AddBlogService(this IServiceCollection services)
        {
            var assembly = typeof(IServiceCollectionExtensions).GetTypeInfo().Assembly;
            var serviceList = assembly.DefinedTypes.Where(t => t.Name.EndsWith("Service") && t.Namespace == "DotNetBlog.Service").ToList();
            foreach (var service in serviceList)
            {
                services.AddScoped(service.AsType());
            }

            services.AddScoped(provider =>
            {
                return provider.GetService<SettingService>().GetAsync().Result;
            });

            services.AddScoped(provider =>
            {
                return provider.GetService<ThemeService>().Get();
            });

            services.AddScoped<ClientManager>();

            return services;
        }

        public static IServiceCollection AddBlogDataContext(this IServiceCollection services, IConfiguration configuration)
        {
            DbContextOptionsBuilder contextOptions;

            switch (configuration["Provider"])
            {
                case "MSSQL":
                    services.AddDbContext<BlogContext, SqlServerBlogContext>(options =>
                        contextOptions = options
                            .UseSqlServer(configuration.GetConnectionString("MSSQL")));
                    break;
                case "PostgreSQL":
                    services.AddDbContext<BlogContext, PostgreeSqlBlogContext>(options =>
                        contextOptions = options
                            .UseNpgsql(configuration.GetConnectionString("PostgreSQL")));
                    break;
                case "MySQL":
                    services.AddDbContext<BlogContext, MySqlBlogContext>(options =>
                        contextOptions = options
                            .UseMySQL(configuration.GetConnectionString("MySQL")));
                    break;
                case "SQLite":
                    services.AddDbContext<BlogContext, SqliteBlogContext>(options =>
                        contextOptions = options
                            .UseSqlite(configuration.GetConnectionString("SQLite")));
                    break;
                default:
                    throw new ArgumentException("Not a valid database type");
            }

            return services;
        }
    }
}
