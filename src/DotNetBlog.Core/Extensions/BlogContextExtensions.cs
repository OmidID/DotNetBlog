﻿using DotNetBlog.Core.Data;
using DotNetBlog.Core.Entity;
using DotNetBlog.Core.Model.Category;
using DotNetBlog.Core.Model.Tag;
using DotNetBlog.Core.Model.Topic;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DotNetBlog.Core.Extensions
{
    public static class BlogContextExtensions
    {
        private static readonly string CacheKey_Category = "Cache_Category";

        private static readonly string CacheKey_Tag = "Cache_Tag";

        private static readonly string CacheKey_MonthStatistics = "Cache_MonthStatistics";

        private static readonly string CacheKey_User = "Cache_User";

        private static readonly string CacheKey_UserToken = "Cache_UserToken";

        public static List<CategoryModel> QueryAllCategoryFromCache(this BlogContext context)
        {
            var result = RetriveCache(context, CacheKey_Category, () =>
            {
                List<Category> entityList = context.Categories.ToList();

                var categoryTopics = context.CategoryTopics.Include(ct => ct.Topic)
                                    .GroupBy(ct => ct.CategoryId)
                                    .Select(ct => new
                                    {
                                        Id = ct.Key,
                                        Total = ct.Count(),
                                        Published =
                                            context.Topics
                                                .Where(w => w.Status == Enums.TopicStatus.Published)
                                                .Count(),
                                        Draft =
                                            context.Topics
                                                .Where(w => w.Status == Enums.TopicStatus.Draft)
                                                .Count()
                                    })
                                    .ToList();

                var query = from entity in entityList
                            join ct in categoryTopics on entity.Id equals ct.Id into temp
                            from ct in temp.DefaultIfEmpty()
                            select new CategoryModel
                            {
                                Id = entity.Id,
                                Name = entity.Name,
                                Description = entity.Description,
                                Topics = new TopicCountModel
                                {
                                    All = ct != null ? ct.Total : 0,
                                    Draft = ct != null ? ct.Draft : 0,
                                    Published = ct != null ? ct.Published : 0
                                }
                            };

                return query.ToList();
            });

            return result;
        }

        public static List<TagModel> QueryAllTagFromCache(this BlogContext context)
        {
            var result = RetriveCache(context, CacheKey_Tag, () =>
            {
                var entityList = context.Tags.ToList();

                var tagTopics = context.TagTopics.Include(ct => ct.Topic)
                                .GroupBy(ct => ct.TagId)
                                .Select(ct => new
                                {
                                    Id = ct.Key,
                                    Total = ct.Count(),
                                    Published = context.TagTopics.Where(t => t.Topic.Status == Enums.TopicStatus.Published).Count(),
                                    Draft = context.TagTopics.Where(t => t.Topic.Status == Enums.TopicStatus.Draft).Count()
                                })
                                .ToList();

                var query = from entity in entityList
                            join tt in tagTopics on entity.Id equals tt.Id into temp
                            from tt in temp.DefaultIfEmpty()
                            select new TagModel
                            {
                                Id = entity.Id,
                                Keyword = entity.Keyword,
                                Topics = new TopicCountModel
                                {
                                    All = tt != null ? tt.Total : 0,
                                    Draft = tt != null ? tt.Draft : 0,
                                    Published = tt != null ? tt.Published : 0
                                }
                            };

                return query.ToList();
            });

            return result;
        }

        public static List<MonthStatisticsModel> QueryMonthStatisticsFromCache(this BlogContext context)
        {
            var result = RetriveCache(context, CacheKey_MonthStatistics, () =>
            {
                var topicList = context.Topics.ToList();

                var query = topicList.GroupBy(g => new { g.EditDate.Year, g.EditDate.Month })
                    .Select(g => new MonthStatisticsModel
                    {
                        Month = new DateTime(g.Key.Year, g.Key.Month, 1),
                        Topics = new TopicCountModel
                        {
                            All = g.Count(),
                            Published = g.Count(topic => topic.Status == Enums.TopicStatus.Published),
                            Draft = g.Count(topic => topic.Status == Enums.TopicStatus.Draft)
                        }
                    });

                return query.ToList();
            }, new MemoryCacheEntryOptions { AbsoluteExpiration = DateTime.Now.AddMinutes(20) });

            return result;
        }

        public static List<User> QueryUserFromCache(this BlogContext context)
        {
            var result = RetriveCache(context, CacheKey_User, () =>
            {
                return context.Users.ToList();
            });

            return result;
        }

        public static void RemoveCategoryCache(this BlogContext context)
        {
            RemoveCache(context, CacheKey_Category);
        }

        public static void RemoveTagCache(this BlogContext context)
        {
            RemoveCache(context, CacheKey_Tag);
        }

        public static void RemoveUserTokenCache(this BlogContext context)
        {
            RemoveCache(context, CacheKey_UserToken);
        }

        public static void RemoveUserCache(this BlogContext context)
        {
            RemoveCache(context, CacheKey_User);
        }

        #region private static methods

        private static T RetriveCache<T>(BlogContext context, string cacheKey, Func<T> func, MemoryCacheEntryOptions options = null)
        {
            var cache = context.ServiceProvider.GetService<IMemoryCache>();
            return cache.RetriveCache<T>(cacheKey, func, options);
        }

        private static void RemoveCache(BlogContext context, string cacheKey)
        {
            var cache = context.ServiceProvider.GetService<IMemoryCache>();
            cache.Remove(cacheKey);
        }

        #endregion
    }
}
