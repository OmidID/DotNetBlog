using DotNetBlog.Core.Data;
using DotNetBlog.Core.Entity;
using DotNetBlog.Core.Enums;
using DotNetBlog.Core.Model;
using DotNetBlog.Core.Model.Install;
using DotNetBlog.Core.Model.Setting;
using DotNetBlog.Core.Model.Widget;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DotNetBlog.Core.Service
{
    public class InstallService
    {
        private static object _sync = new object();
        private static bool? _cacheOfNeedToInstall;

        private BlogContext BlogContext { get; set; }
        private TopicService TopicService { get; set; }
        public CommentService CommentService { get; }
        public ClientManager ClientManager { get; }
        private IServiceProvider ServiceProvider { get; set; }
        private IStringLocalizer<WidgetConfigModelBase> WidgetLocalizer { get; set; }
        private IStringLocalizer<InstallService> InstallLocalizer { get; set; }
        private IStringLocalizer<SettingModel> SettingModelLocalizer { get; set; }
        private IOptions<RequestLocalizationOptions> RequestLocalizationOptions { get; set; }

        public InstallService(BlogContext blogContext,
            TopicService topicService,
            CommentService commentService,
            ClientManager clientManager,
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
        public OperationResult TryInstall(InstallModel model)
        {
            if (!RequestLocalizationOptions.Value.SupportedCultures.Any(t => t.Name.Equals(model.Language)))
            {
                return OperationResult.Failure(InstallLocalizer["Not supported language"]);
            }

            lock (_sync)
            {
                if (!NeedToInstall())
                {
                    return OperationResult.Failure(InstallLocalizer["Blog has been already installed"]);
                }
                else
                {
                    //1. Add admin user
                    AddAdminUser(model);

                    //2. Setup default settings
                    AddSettings(model);

                    //3. Setup widgets
                    AddWidgets(model);

                    this.BlogContext.SaveChanges();

                    //Clear settings cache
                    SettingService settingService = ServiceProvider.GetService<SettingService>();
                    settingService.RemoveCache();

                    //Clear widgets cache
                    WidgetService widgetService = ServiceProvider.GetService<WidgetService>();
                    widgetService.RemoveCache();

                    //4. Add topic
                    AddSampleTopic(model);

                    _cacheOfNeedToInstall = false;

                    return new OperationResult();
                }
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
        private void AddAdminUser(InstallModel model)
        {
            var user = new User
            {
                UserName = model.UserName,
                Password = Utilities.EncryptHelper.MD5(model.Password),
                Nickname = model.Nickname,
                Email = model.Email
            };
            BlogContext.Users.Add(user);
            ClientManager.CurrentUser = user;
        }

        /// <summary>
        /// Add settings
        /// </summary>
        /// <param name="model"></param>
        private void AddSettings(InstallModel model)
        {
            SettingModel settingModel = new SettingModel(new Dictionary<string, string>(), SettingModelLocalizer);
            settingModel.Title = model.BlogTitle;
            settingModel.Host = model.BlogHost;
            settingModel.Language = model.Language;

            var settingList = settingModel.Settings.Select(t => new Setting
            {
                Key = t.Key,
                Value = t.Value
            });

            BlogContext.AddRange(settingList);
        }

        /// <summary>
        /// Add widgets
        /// </summary>
        /// <param name="model"></param>
        private void AddWidgets(InstallModel model)
        {
            IStringLocalizer localizer = WidgetLocalizer.WithCulture(new System.Globalization.CultureInfo(model.Language));

            var widgetList = new List<WidgetModel>();
            widgetList.Add(new WidgetModel
            {
                Type = WidgetType.Administration,
                Config = new AdministrationWidgetConfigModel(localizer)
            });
            widgetList.Add(new WidgetModel
            {
                Type = WidgetType.Search,
                Config = new SearchWidgetConfigModel(localizer)
            });
            widgetList.Add(new WidgetModel
            {
                Type = WidgetType.Category,
                Config = new CategoryWidgetConfigModel(localizer)
            });
            widgetList.Add(new WidgetModel
            {
                Type = WidgetType.Tag,
                Config = new TagWidgetConfigModel(localizer)
            });
            widgetList.Add(new WidgetModel
            {
                Type = WidgetType.MonthStatistics,
                Config = new MonthStatisticeWidgetConfigModel(localizer)
            });
            widgetList.Add(new WidgetModel
            {
                Type = WidgetType.RecentTopic,
                Config = new RecentTopicWidgetConfigModel(localizer)
            });
            widgetList.Add(new WidgetModel
            {
                Type = WidgetType.RecentComment,
                Config = new RecentCommentWidgetConfigModel(localizer)
            });
            widgetList.Add(new WidgetModel
            {
                Type = WidgetType.Page,
                Config = new PageWidgetConfigModel(localizer)
            });

            var widgetEntityList = widgetList.Select(t => new Widget
            {
                Config = JsonConvert.SerializeObject(t.Config),
                Type = t.Type,
                ID = widgetList.IndexOf(t) + 1
            });
            this.BlogContext.AddRange(widgetEntityList);
        }

        private bool AddSampleTopic(InstallModel model)
        {
            if (!model.AddWelcomeTopic) return false;

            var result = TopicService.Add(new Model.Topic.AddTopicModel
            {
                Title = InstallLocalizer["Welcome to DotNetBlog"].Value,
                Content = InstallLocalizer["Welcome topic content"].Value,
                TagList = new string[] { "DotNetBlog" },
                AllowComment = true,
                Status = TopicStatus.Published
            }).Result;

            var commentResult = CommentService.Add(new Model.Comment.AddCommentModel
            {
                TopicID = result.Data.ID,
                Email = model.Email,
                Name = model.UserName,
                Content = InstallLocalizer["Welcome comment"].Value
            }).Result;
            CommentService.ApproveComment(commentResult.Data.ID).Wait();

            return result.Success && commentResult.Success;
        }
    }
}
