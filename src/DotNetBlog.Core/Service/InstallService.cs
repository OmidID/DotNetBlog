using DotNetBlog.Data;
using DotNetBlog.Entity;
using DotNetBlog.Enums;
using DotNetBlog.Model;
using DotNetBlog.Model.Install;
using DotNetBlog.Model.Setting;
using DotNetBlog.Model.Widget;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DotNetBlog.Service
{
    public class InstallService
    {
        private static SemaphoreSlim _lock = new SemaphoreSlim(1, 1);
        private static bool? _cacheOfNeedToInstall;

        private BlogContext BlogContext { get; set; }
        private TopicService TopicService { get; set; }
        public CommentService CommentService { get; }
        public ClientManager ClientManager { get; }
        private UserManager<User> UserManager { get; }
        public RoleManager<UserRole> RoleManager { get; }
        private IServiceProvider ServiceProvider { get; set; }
        private IStringLocalizer<WidgetConfigModelBase> WidgetLocalizer { get; set; }
        private IStringLocalizer<InstallService> InstallLocalizer { get; set; }
        private IStringLocalizer<SettingModel> SettingModelLocalizer { get; set; }
        private IOptions<RequestLocalizationOptions> RequestLocalizationOptions { get; set; }

        public InstallService(BlogContext blogContext,
            TopicService topicService,
            CommentService commentService,
            ClientManager clientManager,
            UserManager<User> userManager,
            RoleManager<UserRole> roleManager,
            IStringLocalizer<WidgetConfigModelBase> widgetLocalizer,
            IServiceProvider serviceProvider,
            IOptions<RequestLocalizationOptions> requestLocalizationOptions,
            IStringLocalizer<InstallService> installLocalizer,
            IStringLocalizer<SettingModel> settingModelLocalizer)
        {
            BlogContext = blogContext;
            TopicService = topicService;
            CommentService = commentService;
            ClientManager = clientManager;
            UserManager = userManager;
            RoleManager = roleManager;
            WidgetLocalizer = widgetLocalizer;
            ServiceProvider = serviceProvider;
            RequestLocalizationOptions = requestLocalizationOptions;
            InstallLocalizer = installLocalizer;
            SettingModelLocalizer = settingModelLocalizer;
        }

        /// <summary>
        /// Try to install the blog
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<OperationResult> TryInstallAsync(InstallModel model)
        {
            await _lock.WaitAsync();

            try
            {
                if (!RequestLocalizationOptions.Value.SupportedCultures.Any(t => t.Name.Equals(model.Language)))
                {
                    return OperationResult.Failure(InstallLocalizer["Not supported language"]);
                }

                if (!NeedToInstall())
                {
                    return OperationResult.Failure(InstallLocalizer["Blog has been already installed"]);
                }
                else
                {
                    using var transaction = BlogContext.Database.BeginTransaction();

                    //1. Add admin user
                    var result = await AddAdminUserAsync(model);
                    if (!result.Success)
                        return result;

                    //2. Setup default settings
                    result = await AddSettingsAsync(model);
                    if (!result.Success)
                        return result;

                    //3. Setup widgets
                    result = await AddWidgetsAsync(model);
                    if (!result.Success)
                        return result;

                    await BlogContext.SaveChangesAsync();

                    //Clear settings cache
                    SettingService settingService = ServiceProvider.GetService<SettingService>();
                    settingService.RemoveCache();

                    //Clear widgets cache
                    WidgetService widgetService = ServiceProvider.GetService<WidgetService>();
                    widgetService.RemoveCache();

                    //4. Add topic
                    result = await AddSampleTopicAsync(model);
                    if (!result.Success)
                        return result;

                    _cacheOfNeedToInstall = false;

                    await transaction.CommitAsync();

                    return new OperationResult();
                }
            }
            finally
            {
                _lock.Release();
            }
        }

        /// <summary>
        /// Check the blog if it is needed to install
        /// </summary>
        /// <returns></returns>
        public bool NeedToInstall() =>
            _cacheOfNeedToInstall ??= !this.BlogContext.Settings.Any();

        /// <summary>
        /// Add admin user
        /// </summary>
        /// <param name="model"></param>
        private async Task<OperationResult> AddAdminUserAsync(InstallModel model)
        {
            var result = await RoleManager.CreateAsync(new UserRole { Name = Policies.AdministratorsRole });
            if (!result.Succeeded)
            {
                return OperationResult.Failure(
                    result.Errors
                        .Select(s => s.Description)
                        .Aggregate((o, n) => $"{o}\n{n}"));
            }

            result = await UserManager.CreateAsync(new User
            {
                UserName = model.UserName,
                Email = model.Email,
                Nickname = model.Nickname,
                EmailConfirmed = true
            }, model.Password);

            if (result.Succeeded)
            {
                var user = await UserManager.FindByNameAsync(model.UserName);

                await ClientManager.InitUser(model.UserName);
                await UserManager.AddToRoleAsync(user, Policies.AdministratorsRole);
                return OperationResult.SuccessResult;
            }

            return OperationResult.Failure(
                result.Errors
                    .Select(s => s.Description)
                    .Aggregate((o, n) => $"{o}\n{n}"));
        }

        /// <summary>
        /// Add settings
        /// </summary>
        /// <param name="model"></param>
        private async Task<OperationResult> AddSettingsAsync(InstallModel model)
        {
            var settingModel = new SettingModel(new Dictionary<string, string>(), SettingModelLocalizer);
            settingModel.Title = model.BlogTitle;
            settingModel.Host = model.BlogHost;
            settingModel.Language = model.Language;
            settingModel.Registration = false;

            var settingList = settingModel.Settings.Select(t => new Setting
            {
                Key = t.Key,
                Value = t.Value
            });

            await BlogContext.AddRangeAsync(settingList);

            return OperationResult.SuccessResult;
        }

        /// <summary>
        /// Add widgets
        /// </summary>
        /// <param name="model"></param>
        private async Task<OperationResult> AddWidgetsAsync(InstallModel model)
        {
            CultureInfo.CurrentCulture = new CultureInfo(model.Language, false);

            var widgetList = new List<WidgetModel>();
            widgetList.Add(new WidgetModel
            {
                Type = WidgetType.Administration,
                Config = new AdministrationWidgetConfigModel(WidgetLocalizer)
            });
            widgetList.Add(new WidgetModel
            {
                Type = WidgetType.Search,
                Config = new SearchWidgetConfigModel(WidgetLocalizer)
            });
            widgetList.Add(new WidgetModel
            {
                Type = WidgetType.Category,
                Config = new CategoryWidgetConfigModel(WidgetLocalizer)
            });
            widgetList.Add(new WidgetModel
            {
                Type = WidgetType.Tag,
                Config = new TagWidgetConfigModel(WidgetLocalizer)
            });
            widgetList.Add(new WidgetModel
            {
                Type = WidgetType.MonthStatistics,
                Config = new MonthStatisticeWidgetConfigModel(WidgetLocalizer)
            });
            widgetList.Add(new WidgetModel
            {
                Type = WidgetType.RecentTopic,
                Config = new RecentTopicWidgetConfigModel(WidgetLocalizer)
            });
            widgetList.Add(new WidgetModel
            {
                Type = WidgetType.RecentComment,
                Config = new RecentCommentWidgetConfigModel(WidgetLocalizer)
            });
            widgetList.Add(new WidgetModel
            {
                Type = WidgetType.Page,
                Config = new PageWidgetConfigModel(WidgetLocalizer)
            });

            var widgetEntityList = widgetList.Select(t => new Widget
            {
                Config = JsonConvert.SerializeObject(t.Config),
                Type = t.Type,
                Id = widgetList.IndexOf(t) + 1
            });

            await this.BlogContext.AddRangeAsync(widgetEntityList);

            return OperationResult.SuccessResult;
        }

        private async Task<OperationResult> AddSampleTopicAsync(InstallModel model)
        {
            if (!model.AddWelcomeTopic) return OperationResult.SuccessResult;

            var result = await TopicService.Add(new Model.Topic.AddTopicModel
            {
                Title = InstallLocalizer["Welcome to DotNetBlog"].Value,
                Content = InstallLocalizer["Welcome topic content"].Value,
                TagList = new string[] { "DotNetBlog" },
                AllowComment = true,
                Status = TopicStatus.Published
            });
            if (!result.Success)
                return result;

            var commentResult = await CommentService.Add(new Model.Comment.AddCommentModel
            {
                TopicId = result.Data.Id,
                Email = model.Email,
                Name = model.UserName,
                Content = InstallLocalizer["Welcome comment"].Value
            });
            await CommentService.ApproveComment(commentResult.Data.Id);

            return commentResult;
        }
    }
}
