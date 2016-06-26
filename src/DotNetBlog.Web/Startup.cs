﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;

namespace DotNetBlog.Web
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit http://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
            services.AddEntityFrameworkSqlite()
                .AddDbContext<Core.Data.BlogContext>(opt =>
                {
                    opt.UseSqlite("DataSource=blog.db", builder => { builder.MigrationsAssembly("DotNetBlog.Web"); });
                });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment enviroment)
        {            
            app.UseStaticFiles();

            app.UseDeveloperExceptionPage();
            app.UseMvc();

            using (var context = app.ApplicationServices.GetService<Core.Data.BlogContext>())
            {
                context.Database.EnsureCreated();
            }
        }
    }
}
