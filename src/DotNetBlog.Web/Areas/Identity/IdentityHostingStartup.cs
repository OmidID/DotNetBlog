using Microsoft.AspNetCore.Hosting;

[assembly: HostingStartup(typeof(DotNetBlog.Web.Areas.Identity.IdentityHostingStartup))]
namespace DotNetBlog.Web.Areas.Identity
{
    public class IdentityHostingStartup : IHostingStartup
    {
        public void Configure(IWebHostBuilder builder)
        {
            builder.ConfigureServices((context, services) =>
            {
            });
        }
    }
}