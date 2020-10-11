using DotNetBlog;
using DotNetBlog.Data;
using DotNetBlog.Entity;
using DotNetBlog.Web.ViewEngines;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using System.Collections.Generic;
using System.Globalization;

namespace DotNetBlog.Web
{
    public static class IServiceCollectionExtensions
    {
        public static IServiceCollection AddDotNetBlog(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddLocalization(opts => { opts.ResourcesPath = "Resources"; });

            services.AddSingleton(configuration);
            services.AddMvc()
                .AddViewLocalization(
                    LanguageViewLocationExpanderFormat.Suffix,
                    opts => { opts.ResourcesPath = "Resources"; })
                .AddDataAnnotationsLocalization()
                .AddNewtonsoftJson();

            services.AddMemoryCache();

            services
                .AddAutoMapper()
                .AddBlogDataContext(configuration)
                .AddBlogService()
                .AddAuthenticationAndExternal(configuration)
                .AddMvcAndSpa()
                .AddMultiLanguage()
                .AddThemeEngine()
                .AddPolicies();

            return services;
        }

        public static IServiceCollection AddAuthenticationAndExternal(this IServiceCollection services, IConfiguration configuration)
        {
            var authContext = services.AddAuthentication();
            if (!string.IsNullOrEmpty(configuration["Authentication:Microsoft:ClientId"]))
            {
                authContext.AddMicrosoftAccount(microsoftOptions =>
                {
                    microsoftOptions.ClientId = configuration["Authentication:Microsoft:ClientId"];
                    microsoftOptions.ClientSecret = configuration["Authentication:Microsoft:ClientSecret"];
                });
            }

            if (!string.IsNullOrEmpty(configuration["Authentication:Google:ClientId"]))
            {
                authContext.AddGoogle(options =>
                {
                    IConfigurationSection googleAuthNSection =
                        configuration.GetSection("Authentication:Google");

                    options.ClientId = googleAuthNSection["ClientId"];
                    options.ClientSecret = googleAuthNSection["ClientSecret"];
                });
            }

            if (!string.IsNullOrEmpty(configuration["Authentication:Google:ClientId"]))
            {
                authContext.AddFacebook(facebookOptions =>
                {
                    facebookOptions.AppId = configuration["Authentication:Facebook:AppId"];
                    facebookOptions.AppSecret = configuration["Authentication:Facebook:AppSecret"];
                });
            }

            services.AddDefaultIdentity<User>(options =>
            {
                options.SignIn.RequireConfirmedAccount = true;
            })
            .AddRoles<UserRole>()
            .AddEntityFrameworkStores<BlogContext>();

            return services;
        }

        public static IServiceCollection AddMvcAndSpa(this IServiceCollection services)
        {
            services.AddControllersWithViews();
            services.AddRazorPages();
            services.AddServerSideBlazor();

            // services.AddSpaStaticFiles(configuration =>
            // {
            //     configuration.RootPath = "wwwroot/dist";
            // });

            return services;
        }

        public static IServiceCollection AddMultiLanguage(this IServiceCollection services)
        {
            services.Configure<RequestLocalizationOptions>(opts =>
            {
                var supportedCultures = new List<CultureInfo>
                {
                    new CultureInfo("en-GB"),
                    new CultureInfo("zh-CN")
                };

                opts.DefaultRequestCulture = new RequestCulture("en-GB");
                // Formatting numbers, dates, etc.
                opts.SupportedCultures = supportedCultures;
                // UI strings that we have localized.
                opts.SupportedUICultures = supportedCultures;

                //Uncomment for change language by user
                opts.RequestCultureProviders.Insert(0, new CustomRequestCultureProvider(async context =>
                {
                    var settingService = context.RequestServices.GetService<Service.SettingService>();
                    var setting = await settingService.GetAsync();
                    // My custom request culture logic
                    return new ProviderCultureResult(setting.Language);
                }));
            });

            return services;
        }

        public static IServiceCollection AddThemeEngine(this IServiceCollection services)
        {
            services.AddSingleton<IStringLocalizerFactory, ThemeResourceLocalizationFactory>();
            services.Configure<RazorViewEngineOptions>(options =>
            {
                options.ViewLocationExpanders.Add(new ThemeViewEngine());
            });

            return services;
        }

        public static IServiceCollection AddPolicies(this IServiceCollection services)
        {
            services.AddAuthorization(options =>
            {
                options.AddPolicy(Policies.AdminAccess, policy =>
                    policy.RequireRole(Policies.AdministratorsRole));
            });

            return services;
        }
    }
}
