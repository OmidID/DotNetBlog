﻿using AutoMapper;
using DotNetBlog.Core.Data;
using DotNetBlog.Core.Entity;
using DotNetBlog.Core.Extensions;
using DotNetBlog.Core.Model;
using DotNetBlog.Core.Model.Category;
using DotNetBlog.Core.Model.Setting;
using DotNetBlog.Core.Model.Topic;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DotNetBlog.Core.Service
{
    public class TopicService
    {
        private readonly IMapper _mapper;

        private BlogContext BlogContext { get; set; }

        private IMemoryCache Cache { get; set; }

        private SettingModel Settings { get; set; }

        private ClientManager ClientManager { get; set; }

        private IHtmlLocalizer<TopicService> L { get; set; }

        public TopicService(BlogContext blogContext,
            IMemoryCache cache,
            SettingModel settings,
            ClientManager clientManager,
            IHtmlLocalizer<TopicService> localizer,
            IMapper mapper)
        {
            BlogContext = blogContext;
            Cache = cache;
            Settings = settings;
            ClientManager = clientManager;
            L = localizer;
            _mapper = mapper;
        }

        public async Task<OperationResult<TopicModel>> Add(AddTopicModel model)
        {
            model.CategoryList = (model.CategoryList ?? new int[0]).Distinct().ToArray();
            model.TagList = (model.TagList ?? new string[0]).Distinct().ToArray();

            List<Category> categoryEntityList = await BlogContext.Categories.Where(t => model.CategoryList.Contains(t.Id)).ToListAsync();
            List<Tag> tagEntityList = await BlogContext.Tags.Where(t => model.TagList.Contains(t.Keyword)).ToListAsync();

            model.Alias = await this.GenerateAlias(null, model.Alias, model.Title);
            model.Summary = this.GenerateSummary(model.Summary, model.Content);

            foreach (var tag in model.TagList)
            {
                if (!tagEntityList.Any(t => t.Keyword == tag))
                {
                    var tagEntity = new Tag
                    {
                        Keyword = tag
                    };
                    BlogContext.Tags.Add(tagEntity);
                    tagEntityList.Add(tagEntity);
                }
            }

            var topic = new Topic
            {
                Alias = model.Alias,
                AllowComment = model.AllowComment,
                Content = model.Content,
                CreateDate = DateTime.Now,
                CreateUserId = this.ClientManager.CurrentUser.Id,
                EditDate = model.Date ?? DateTime.Now,
                EditUserId = 1,
                Status = model.Status,
                Summary = model.Summary,
                Title = model.Title
            };
            BlogContext.Topics.Add(topic);

            List<CategoryTopic> categoryTopicList = categoryEntityList.Select(t => new CategoryTopic
            {
                Category = t,
                Topic = topic
            }).ToList();
            BlogContext.CategoryTopics.AddRange(categoryTopicList);

            List<TagTopic> tagTopicList = tagEntityList.Select(t => new TagTopic
            {
                Tag = t,
                Topic = topic
            }).ToList();
            BlogContext.TagTopics.AddRange(tagTopicList);

            await BlogContext.SaveChangesAsync();

            BlogContext.RemoveCategoryCache();
            BlogContext.RemoveTagCache();

            var topicModel = (await this.Transform(topic)).First();

            return new OperationResult<TopicModel>(topicModel);
        }

        public async Task<OperationResult<TopicModel>> Edit(EditTopicModel model)
        {
            var entity = await BlogContext.Topics.SingleOrDefaultAsync(t => t.Id == model.Id);
            if (entity == null)
            {
                return OperationResult<TopicModel>.Failure(L["The article does not exists"].Value);
            }

            using (var tran = await BlogContext.Database.BeginTransactionAsync())
            {
                List<CategoryTopic> deletedCategoryTopicList = await BlogContext.CategoryTopics.Where(t => t.TopicId == model.Id).ToListAsync();
                BlogContext.RemoveRange(deletedCategoryTopicList);
                List<TagTopic> deletedTagTopicList = await BlogContext.TagTopics.Where(t => t.TopicId == model.Id).ToListAsync();
                BlogContext.RemoveRange(deletedTagTopicList);

                await BlogContext.SaveChangesAsync();

                model.CategoryList = (model.CategoryList ?? new int[0]).Distinct().ToArray();
                model.TagList = (model.TagList ?? new string[0]).Distinct().ToArray();

                List<Category> categoryEntityList = await BlogContext.Categories.Where(t => model.CategoryList.Contains(t.Id)).ToListAsync();
                List<Tag> tagEntityList = await BlogContext.Tags.Where(t => model.TagList.Contains(t.Keyword)).ToListAsync();

                model.Alias = await this.GenerateAlias(model.Id, model.Alias, model.Title);
                model.Summary = this.GenerateSummary(model.Summary, model.Content);

                foreach (var tag in model.TagList)
                {
                    if (!tagEntityList.Any(t => t.Keyword == tag))
                    {
                        var tagEntity = new Tag
                        {
                            Keyword = tag
                        };
                        BlogContext.Tags.Add(tagEntity);
                        tagEntityList.Add(tagEntity);
                    }
                }

                entity.Title = model.Title;
                entity.Content = model.Content;
                entity.Status = model.Status;
                entity.Alias = model.Alias;
                entity.Summary = model.Summary;
                entity.EditDate = model.Date ?? DateTime.Now;
                entity.AllowComment = model.AllowComment;

                List<CategoryTopic> categoryTopicList = categoryEntityList.Select(t => new CategoryTopic
                {
                    Category = t,
                    Topic = entity
                }).ToList();
                BlogContext.CategoryTopics.AddRange(categoryTopicList);

                List<TagTopic> tagTopicList = tagEntityList.Select(t => new TagTopic
                {
                    Tag = t,
                    Topic = entity
                }).ToList();
                BlogContext.TagTopics.AddRange(tagTopicList);

                await BlogContext.SaveChangesAsync();

                tran.Commit();
            }

            BlogContext.RemoveCategoryCache();
            BlogContext.RemoveTagCache();

            var topicModel = (await this.Transform(entity)).First();

            return new OperationResult<TopicModel>(topicModel);
        }

        /// <summary>
        /// 查询正常状态的文章列表
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="status"></param>
        /// <param name="keywords"></param>
        /// <returns></returns>
        public async Task<PagedResult<TopicModel>> QueryNotTrash(int pageIndex, int pageSize, Enums.TopicStatus? status, string keywords)
        {
            var query = BlogContext.Topics.AsNoTracking().Where(t => t.Status != Enums.TopicStatus.Trash);

            if (status.HasValue)
            {
                query = query.Where(t => t.Status == status.Value);
            }
            if (!string.IsNullOrWhiteSpace(keywords))
            {
                query = query.Where(t => t.Title.Contains(keywords));
            }

            int total = await query.CountAsync();

            Topic[] entityList = await query.OrderByDescending(t => t.EditDate).Skip((pageIndex - 1) * pageSize).Take(pageSize).ToArrayAsync();

            List<TopicModel> modelList = await Transform(entityList);

            return new PagedResult<TopicModel>(modelList, total);
        }

        /// <summary>
        /// 根据分类，查询文章列表
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="categoryId"></param>
        /// <returns></returns>
        public async Task<PagedResult<TopicModel>> QueryByCategory(int pageIndex, int pageSize, int categoryId)
        {
            var topicIdQuery = BlogContext.CategoryTopics.Where(t => t.CategoryId == categoryId).Select(t => t.TopicId);
            var query = BlogContext.Topics.Where(t => t.Status == Enums.TopicStatus.Published && topicIdQuery.Contains(t.Id));

            int total = await query.CountAsync();

            Topic[] entityList = await query.AsNoTracking().OrderByDescending(t => t.EditDate).Skip((pageIndex - 1) * pageSize).Take(pageSize).ToArrayAsync();

            List<TopicModel> modelList = await Transform(entityList);

            return new PagedResult<TopicModel>(modelList, total);
        }

        /// <summary>
        /// 根据标签，查询文章列表
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="keyword"></param>
        /// <returns></returns>
        public async Task<PagedResult<TopicModel>> QueryByTag(int pageIndex, int pageSize, string keyword)
        {
            var topicIdQuery = from tag in BlogContext.Tags
                               where tag.Keyword == keyword
                               join tagTopic in BlogContext.TagTopics on tag.Id equals tagTopic.TagId
                               select tagTopic.TopicId;

            var query = BlogContext.Topics.Where(t => t.Status == Enums.TopicStatus.Published && topicIdQuery.Contains(t.Id));

            int total = await query.CountAsync();

            Topic[] entityList = await query.AsNoTracking().OrderByDescending(t => t.EditDate).Skip((pageIndex - 1) * pageSize).Take(pageSize).ToArrayAsync();

            List<TopicModel> modelList = await Transform(entityList);

            return new PagedResult<TopicModel>(modelList, total);
        }

        /// <summary>
        /// 根据月份，查询文章列表
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="year"></param>
        /// <param name="month"></param>
        /// <returns></returns>
        public async Task<PagedResult<TopicModel>> QueryByMonth(int pageIndex, int pageSize, int year, int month)
        {
            var startDate = new DateTime(year, month, 1, 0, 0, 0);
            var endDate = startDate.AddMonths(1);

            var query = BlogContext.Topics.Where(t => t.Status == Enums.TopicStatus.Published)
                .Where(t => t.EditDate >= startDate && t.EditDate < endDate);

            int total = await query.CountAsync();

            Topic[] entityList = await query.AsNoTracking().OrderByDescending(t => t.EditDate).Skip((pageIndex - 1) * pageSize).Take(pageSize).ToArrayAsync();

            List<TopicModel> modelList = await Transform(entityList);

            return new PagedResult<TopicModel>(modelList, total);
        }

        /// <summary>
        /// 根据关键字，查询文章列表
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="keywords"></param>
        /// <returns></returns>
        public async Task<PagedResult<TopicModel>> QueryByKeywords(int pageIndex, int pageSize, string keywords)
        {
            var query = BlogContext.Topics.Where(t => t.Status == Enums.TopicStatus.Published)
                .Where(t => t.Title.Contains(keywords) || t.Summary.Contains(keywords) || t.Content.Contains(keywords));

            int total = await query.CountAsync();

            Topic[] entityList = await query.AsNoTracking().OrderByDescending(t => t.EditDate).Skip((pageIndex - 1) * pageSize).Take(pageSize).ToArrayAsync();

            List<TopicModel> modelList = await Transform(entityList);

            return new PagedResult<TopicModel>(modelList, total);
        }

        /// <summary>
        /// 查询最新发布的文章
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        public async Task<List<TopicModel>> QueryRecent(int count, int? categoryId)
        {
            var query = BlogContext.Topics.AsNoTracking().Where(t => t.Status == Enums.TopicStatus.Published);

            if (categoryId.HasValue)
            {
                var topicIdQuery = BlogContext.CategoryTopics.AsNoTracking().Where(t => t.CategoryId == categoryId.Value)
                    .Select(t => t.TopicId);
                query = query.Where(t => topicIdQuery.Contains(t.Id));
            }

            query = query.OrderByDescending(t => t.EditDate).Take(count);

            Topic[] entityList = await query.ToArrayAsync();

            List<TopicModel> modelList = await Transform(entityList);

            return modelList;
        }

        /// <summary>
        /// 得到文章实体
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<TopicModel> Get(int id)
        {
            var entity = await BlogContext.Topics.AsNoTracking().SingleOrDefaultAsync(t => t.Id == id);

            if (entity == null)
            {
                return null;
            }

            return (await Transform(entity)).First();
        }

        /// <summary>
        /// 根据别名，得到文章实体
        /// </summary>
        /// <param name="alias"></param>
        /// <returns></returns>
        public async Task<TopicModel> Get(string alias)
        {
            var entity = await BlogContext.Topics.AsNoTracking().SingleOrDefaultAsync(t => t.Alias == alias);

            if (entity == null)
            {
                return null;
            }

            return (await Transform(entity)).First();
        }

        /// <summary>
        /// 得到前一篇文章
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<TopicModel> GetPrev(TopicModel topic)
        {
            var entity = await BlogContext.Topics.AsNoTracking().Where(t => t.Status == Enums.TopicStatus.Published && t.EditDate < topic.Date)
                .OrderByDescending(t => t.EditDate)
                .FirstOrDefaultAsync();

            if (entity == null)
            {
                return null;
            }

            return (await Transform(entity)).First();
        }

        /// <summary>
        /// 得到下一篇文章
        /// </summary>
        /// <param name="topic"></param>
        /// <returns></returns>
        public async Task<TopicModel> GetNext(TopicModel topic)
        {
            var entity = await BlogContext.Topics.AsNoTracking().Where(t => t.Status == Enums.TopicStatus.Published && t.EditDate > topic.Date)
                .OrderBy(t => t.EditDate)
                .FirstOrDefaultAsync();

            if (entity == null)
            {
                return null;
            }

            return (await Transform(entity)).First();
        }

        /// <summary>
        /// 查询关联文章
        /// </summary>
        /// <param name="topic"></param>
        /// <returns></returns>
        public async Task<List<TopicModel>> QueryRelated(TopicModel topic)
        {
            string cacheKey = $"Cache_RelatedTopic_{topic.Id}";

            var list = Cache.Get<List<TopicModel>>(cacheKey);
            if (list == null)
            {
                if (topic.Tags.Length == 0 && topic.Categories.Length == 0)
                {
                    list = new List<TopicModel>();
                    Cache.Set(cacheKey, list);
                    return list;
                }

                var query = BlogContext.Topics.AsNoTracking().Where(t => t.Status == Enums.TopicStatus.Published && t.Id != topic.Id);
                if (topic.Tags.Length > 0)
                {
                    var topicIdQuery = from tag in BlogContext.Tags.AsNoTracking()
                                       where topic.Tags.Contains(tag.Keyword)
                                       join tagTopic in BlogContext.TagTopics on tag.Id equals tagTopic.TagId
                                       select tagTopic.TopicId;

                    query = query.Where(t => topicIdQuery.Contains(t.Id));
                }
                if (topic.Categories.Length > 0)
                {
                    var categoryIdList = topic.Categories.Select(t => t.Id).ToArray();
                    var topicIdQuery = from category in BlogContext.Categories.AsNoTracking()
                                       where categoryIdList.Contains(category.Id)
                                       join categoryTopic in BlogContext.CategoryTopics on category.Id equals categoryTopic.CategoryId
                                       select categoryTopic.TopicId;

                    query = query.Where(t => topicIdQuery.Contains(t.Id));
                }

                var entityList = await query.OrderByDescending(t => t.EditDate).Take(10).ToArrayAsync();

                list = await Transform(entityList);

                Cache.Set(cacheKey, list);
            }

            return list;
        }

        /// <summary>
        /// 得到按月份的文章统计结果
        /// </summary>
        /// <returns></returns>
        public async Task<List<MonthStatisticsModel>> QueryMonthStatistics()
        {
            var list = BlogContext.QueryMonthStatisticsFromCache();
            return await Task.FromResult(list);
        }

        /// <summary>
        /// 批量修改文章的状态
        /// </summary>
        /// <param name="idList"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        public async Task BatchUpdateStatus(int[] idList, Enums.TopicStatus status)
        {
            var topicList = await BlogContext.Topics.Where(t => idList.Contains(t.Id)).ToListAsync();

            topicList.ForEach(topic =>
            {
                topic.Status = status;
            });

            await BlogContext.SaveChangesAsync();

            BlogContext.RemoveCategoryCache();
            BlogContext.RemoveTagCache();
        }

        private async Task<List<TopicModel>> Transform(params Topic[] entityList)
        {
            if (entityList == null)
            {
                return null;
            }

            int[] idList = entityList.Select(t => t.Id).ToArray();

            var categoryTopicList = await BlogContext.CategoryTopics.Include(t => t.Category).Where(t => idList.Contains(t.TopicId)).ToListAsync();
            var tagTopicList = await BlogContext.TagTopics.Include(t => t.Tag).Where(t => idList.Contains(t.TopicId)).ToListAsync();
            var topicComments = await BlogContext.Comments.Where(t => idList.Contains(t.TopicId))
                                .GroupBy(t => t.TopicId)
                                .Select(t => new
                                {
                                    TopicId = t.Key,
                                    Approved = BlogContext.Comments.Where(c => c.Status == Enums.CommentStatus.Approved).Count(),
                                    Reject = BlogContext.Comments.Where(c => c.Status == Enums.CommentStatus.Reject).Count(),
                                    Pending = BlogContext.Comments.Where(c => c.Status == Enums.CommentStatus.Pending).Count(),
                                    Total = t.Count()
                                }).ToListAsync();

            List<TopicModel> result = entityList.Select(entity =>
            {
                var model = _mapper.Map<TopicModel>(entity);
                model.Categories = categoryTopicList.Where(category => category.TopicId == entity.Id)
                    .Select(category => new CategoryBasicModel
                    {
                        Id = category.CategoryId,
                        Name = category.Category.Name
                    }).ToArray();
                model.Tags = tagTopicList.Where(tag => tag.TopicId == entity.Id)
                    .Select(tag => tag.Tag.Keyword)
                    .ToArray();
                model.Comments = new Model.Comment.CommentCountModel();

                var topicComment = topicComments.SingleOrDefault(t => t.TopicId == entity.Id);
                if (topicComment != null)
                {
                    model.Comments.Approved = topicComment.Approved;
                    model.Comments.Pending = topicComment.Pending;
                    model.Comments.Reject = topicComment.Reject;
                    model.Comments.Total = topicComment.Total;
                }

                return model;
            }).ToList();

            return result;
        }

        private async Task<string> GenerateAlias(int? id, string alias, string title)
        {
            string r_alias = alias.TrimNotCharater();
            if (string.IsNullOrWhiteSpace(r_alias))
            {
                r_alias = title.TrimNotCharater();
            }

            if (!string.IsNullOrWhiteSpace(r_alias))
            {
                var query = this.BlogContext.Topics.Where(t => t.Alias == r_alias);
                if (id.HasValue)
                {
                    query = query.Where(t => t.Id != id.Value);
                }

                if (await query.AnyAsync())
                {
                    r_alias = null;
                }
            }
            else
            {
                r_alias = null;
            }

            return r_alias;
        }

        private string GenerateSummary(string summary, string content)
        {
            if (string.IsNullOrWhiteSpace(summary))
            {
                summary = content;
            }

            if (string.IsNullOrWhiteSpace(summary))
            {
                return string.Empty;
            }

            return summary.FromMarkdown().TrimHtml().ToLength(200);
        }

        public bool CanComment(Topic entity)
        {
            if (entity == null || entity.Status != Enums.TopicStatus.Published || !entity.AllowComment)
            {
                return false;
            }
            if (!this.Settings.AllowComment)
            {
                return false;
            }
            if (this.Settings.CloseCommentDays > 0)
            {
                if (entity.EditDate.AddDays(this.Settings.CloseCommentDays) < DateTime.Now)
                {
                    return false;
                }
            }

            return true;
        }

        public bool CanComment(TopicModel topic)
        {
            if (topic == null || topic.Status != Enums.TopicStatus.Published || !topic.AllowComment)
            {
                return false;
            }
            if (!this.Settings.AllowComment)
            {
                return false;
            }
            if (this.Settings.CloseCommentDays > 0)
            {
                if (topic.Date.AddDays(this.Settings.CloseCommentDays) < DateTime.Now)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
