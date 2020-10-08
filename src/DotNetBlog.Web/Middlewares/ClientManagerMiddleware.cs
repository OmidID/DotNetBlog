﻿using DotNetBlog;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;

namespace DotNetBlog.Web.Middlewares
{
    // You may need to install the Microsoft.AspNetCore.Http.Abstractions package into your project
    public class ClientManagerMiddleware
    {
        private readonly RequestDelegate _next;

        public ClientManagerMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            var clientManager = httpContext.RequestServices.GetService<ClientManager>();
            await clientManager.Init(httpContext);

            await _next(httpContext);
        }
    }

    // Extension method used to add the middleware to the HTTP request pipeline.
    public static class ClientManagerMiddlewareExtensions
    {
        public static IApplicationBuilder UseClientManager(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ClientManagerMiddleware>();
        }
    }
}
