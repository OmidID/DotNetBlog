using DotNetBlog.Data;
using DotNetBlog.Web.Middlewares;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using MimeKit.Cryptography;
using System.IO;

namespace DotNetBlog.Web
{
    public static class IApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseDotNetBlog(this IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            var uploadFolder = env.ContentRootPath + "/App_Data/upload";
            if (!Directory.Exists(uploadFolder))
                Directory.CreateDirectory(uploadFolder);

            app.UseStaticFiles();
            app.UseStaticFiles(new StaticFileOptions
            {
                RequestPath = new PathString("/upload"),
                FileProvider = new PhysicalFileProvider(uploadFolder)
            });

            app
                .UseRouting()
                .UseAuth()
                .UseClientManager()
                .UserDotNetBlogDataContext()
                .UseLanguage()
                .UseDotNetBlogEndpoints()
                .UseDotNetBlogSpa(env);

            return app;
        }

        public static IApplicationBuilder UseLanguage(this IApplicationBuilder app)
        {
            var options = app.ApplicationServices.GetService<IOptions<RequestLocalizationOptions>>();
            app.UseRequestLocalization(options.Value);

            return app;
        }

        public static IApplicationBuilder UseAuth(this IApplicationBuilder app)
        {
            app.UseAuthentication();
            app.UseAuthorization();

            return app;
        }

        public static IApplicationBuilder UserDotNetBlogDataContext(this IApplicationBuilder app)
        {
            using (var scope = app.ApplicationServices.CreateScope())
            {
                var blogContext = scope.ServiceProvider.GetService<BlogContext>();
                // blogContext.Database.EnsureCreated();
                blogContext.Database.Migrate();
            }

            return app;
        }

        public static IApplicationBuilder UseDotNetBlogEndpoints(this IApplicationBuilder app)
        {
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "areaRoute",
                    pattern: "{area:exists}/{controller}/{action=Index}/{id?}");

                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
                
                endpoints.MapRazorPages();
            });

            app.Map("/manager", child =>
            {
                child.UseBlazorFrameworkFiles();
                child.UseStaticFiles();
                child.UseRouting();
                child.UseEndpoints(endpoints =>
                {
                    endpoints.MapFallbackToController("Index", "Manager");
                    endpoints.MapBlazorHub();
                });
            });

            return app;
        }

        public static IApplicationBuilder UseDotNetBlogSpa(this IApplicationBuilder app, IWebHostEnvironment env)
        {
            // if (env.IsDevelopment())
            // {
            //     app.UseSpa(spa =>
            //     {
            //         spa.Options.SourcePath = "../DotNetBlog.Admin";
            //         spa.UseReactDevelopmentServer(npmScript: "start");
            //     });
            // }

            return app;
        }
    }
}
