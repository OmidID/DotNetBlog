using DotNetBlog.Data;
using DotNetBlog.Entity;
using DotNetBlog.Enums;
using DotNetBlog.Extensions;
using DotNetBlog.Model;
using DotNetBlog.Model.Widget;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DotNetBlog.Service
{
    public class WidgetService
    {
        private static readonly string CacheKey = "Cache_Widget";

        private static readonly Dictionary<WidgetType, Type> DefaultWidgetConfigTypes = new Dictionary<WidgetType, Type>
        {
            { WidgetType.Administration, typeof(AdministrationWidgetConfigModel) },
            { WidgetType.Category, typeof(CategoryWidgetConfigModel) },
            { WidgetType.RecentComment, typeof(RecentCommentWidgetConfigModel) },
            { WidgetType.MonthStatistics, typeof(MonthStatisticeWidgetConfigModel) },
            { WidgetType.Page, typeof(PageWidgetConfigModel) },
            { WidgetType.Search, typeof(SearchWidgetConfigModel) },
            { WidgetType.Tag, typeof(TagWidgetConfigModel) },
            { WidgetType.RecentTopic, typeof(RecentTopicWidgetConfigModel) },
            { WidgetType.Link, typeof(LinkWidgetConfigModel) }
        };

        private BlogContext BlogContext { get; set; }

        private IMemoryCache Cache { get; set; }

        private IStringLocalizer<WidgetConfigModelBase> L { get; set; }

        public WidgetService(BlogContext blogContext, IMemoryCache cache, IStringLocalizer<WidgetConfigModelBase> localizer)
        {
            this.BlogContext = blogContext;
            this.Cache = cache;
            this.L = localizer;
        }

        public List<AvailableWidgetModel> QueryAvailable()
        {
            List<AvailableWidgetModel> result = new List<AvailableWidgetModel>();

            var arr = Enum.GetValues(typeof(WidgetType));
            foreach (byte item in arr)
            {
                var type = (WidgetType)item;
                var configType = DefaultWidgetConfigTypes[type];
                var instance = (WidgetConfigModelBase)Activator.CreateInstance(configType, L);
                result.Add(new AvailableWidgetModel
                {
                    Type = type,
                    Name = instance.Title,
                    DefaultConfig = instance,
                    Icon = type.ToString()
                });
            }

            result = result.OrderBy(t => t.Type).ToList();

            return result;
        }

        private async Task<List<Widget>> All()
        {
            var result = await this.Cache.RetriveCacheAsync(CacheKey, async () =>
            {
                var list = await this.BlogContext.Widgets.ToListAsync();
                return list;
            });

            return result;
        }

        public async Task<List<WidgetModel>> Query()
        {
            var entityList = await this.All();

            var result = entityList.OrderBy(t => t.Id).Select(t => new WidgetModel
            {
                Type = t.Type,
                Config = JsonConvert.DeserializeObject(t.Config, DefaultWidgetConfigTypes[t.Type]) as WidgetConfigModelBase
            });

            return result.ToList();
        }

        public async Task<OperationResult> Save(List<WidgetModel> widgetList)
        {
            using var tran = await BlogContext.Database.BeginTransactionAsync();
            var entityList = await BlogContext.Widgets.ToListAsync();

            BlogContext.RemoveRange(entityList);
            await BlogContext.SaveChangesAsync();

            entityList = widgetList.Select(t => new Widget
            {
                Type = t.Type,
                Id = widgetList.IndexOf(t) + 1,
                Config = JsonConvert.SerializeObject(t.Config)
            }).ToList();

            await BlogContext.AddRangeAsync(entityList);
            await BlogContext.SaveChangesAsync();

            await tran.CommitAsync();

            RemoveCache();

            return new OperationResult();
        }

        public WidgetConfigModelBase Transform(WidgetType type, JObject config)
        {
            Type targetType = DefaultWidgetConfigTypes[type];
            return config.ToObject(targetType) as WidgetConfigModelBase;
        }

        public void RemoveCache()
        {
            this.Cache.Remove(CacheKey);
        }
    }
}
