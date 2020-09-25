using DotNetBlog.Core.Data;
using DotNetBlog.Core.Entity;
using DotNetBlog.Core.Extensions;
using DotNetBlog.Core.Model;
using DotNetBlog.Core.Model.Comment;
using DotNetBlog.Core.Model.Setting;
using DotNetBlog.Core.Model.Topic;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DotNetBlog.Core.Service
{
    public class CommentService
    {
        private BlogContext BlogContext { get; set; }

        private ClientManager ClientManager { get; set; }

        private IMemoryCache Cache { get; set; }

        private IServiceProvider ServiceProvider { get; set; }

        private SettingModel Settings { get; set; }

        private EmailService EmailService { get; set; }

        private IHtmlLocalizer<CommentService> L { get; set; }

        public CommentService(BlogContext blogContext,
            ClientManager clientManager,
            IMemoryCache cache,
            IServiceProvider serviceProvider,
            SettingModel settings,
            EmailService emailService,
            IHtmlLocalizer<CommentService> localizer)
        {
            this.BlogContext = blogContext;
            this.ClientManager = clientManager;
            this.Cache = cache;
            this.ServiceProvider = serviceProvider;
            this.Settings = settings;
            this.EmailService = emailService;
            this.L = localizer;
        }

        public async Task<OperationResult<CommentModel>> Add(AddCommentModel model)
        {
            var topic = await BlogContext.Topics.SingleOrDefaultAsync(t => t.Id == model.TopicId);
            if (topic == null || topic.Status != Enums.TopicStatus.Published)
            {
                return OperationResult<CommentModel>.Failure(L["Article does not exists"].Value);
            }

            var topicService = this.ServiceProvider.GetService<TopicService>();
            if (!topicService.CanComment(topic))
            {
                return OperationResult<CommentModel>.Failure(L["Comments not allow for this articles"].Value);
            }

            Comment replyEntity = null;
            if (model.ReplyTo.HasValue)
            {
                replyEntity = await BlogContext.Comments.SingleOrDefaultAsync(t => t.Id == model.ReplyTo.Value);

                if (replyEntity == null || replyEntity.Status != Enums.CommentStatus.Approved)
                {
                    return OperationResult<CommentModel>.Failure(L["Reply to comment not available"].Value);
                }

                if (replyEntity.TopicId != model.TopicId)
                {
                    return OperationResult<CommentModel>.Failure(L["Wrong reply object"].Value);
                }
            }

            Enums.CommentStatus status;
            if (this.Settings.VerifyComment)
            {
                status = Enums.CommentStatus.Pending;

                if (this.Settings.TrustAuthenticatedCommentUser)
                {
                    if (await BlogContext.Comments.AnyAsync(t => t.Email == model.Email && t.Status == Enums.CommentStatus.Approved))
                    {
                        status = Enums.CommentStatus.Approved;
                    }
                }
            }
            else
            {
                status = Enums.CommentStatus.Approved;
            }

            var entity = new Comment
            {
                Content = model.Content,
                CreateDate = DateTime.Now,
                CreateIP = this.ClientManager.ClientIP,
                Email = model.Email,
                Name = model.Name,
                NotifyOnComment = model.NotifyOnComment,
                ReplyToId = model.ReplyTo,
                TopicId = model.TopicId.Value,
                Status = status,
                UserId = this.ClientManager.CurrentUser?.Id,
                WebSite = model.WebSite
            };

            BlogContext.Comments.Add(entity);
            await BlogContext.SaveChangesAsync();
            SendEmail(topic, replyEntity, entity);

            var commentModel = Transform(entity).First();

            return new OperationResult<CommentModel>(commentModel);
        }

        private async void SendEmail(Topic topic, Comment replyEntity, Comment entity)
        {
            await this.EmailService.SendCommentEmail(topic, entity);
            if (entity.Status == Enums.CommentStatus.Approved && entity.ReplyToId.HasValue && replyEntity.NotifyOnComment)
            {
                await this.EmailService.SendReplyEmail(topic, entity, replyEntity);
            }
        }

        public async Task<OperationResult<CommentModel>> DirectlyReply(int replyTo, string content)
        {
            Comment comment = await BlogContext.Comments.SingleOrDefaultAsync(t => t.Id == replyTo);

            if (comment == null)
            {
                return OperationResult<CommentModel>.Failure(L["Comment not exists"].Value);
            }

            var entity = new Comment
            {
                Content = content,
                CreateDate = DateTime.Now,
                CreateIP = this.ClientManager.ClientIP,
                Email = this.ClientManager.CurrentUser.Email,
                Name = this.ClientManager.CurrentUser.Nickname,
                ReplyToId = replyTo,
                Status = Enums.CommentStatus.Approved,
                TopicId = comment.TopicId,
                UserId = this.ClientManager.CurrentUser.Id
            };

            this.BlogContext.Add(entity);
            await this.BlogContext.SaveChangesAsync();

            var commentModel = Transform(entity).First();
            return new OperationResult<CommentModel>(commentModel);
        }

        public async Task<List<CommentModel>> QueryByTopic(int topicId)
        {
            var query = BlogContext.Comments.AsNoTracking().Where(t => t.TopicId == topicId);
            if (ClientManager.IsLogin)
            {
                query = query.Where(t => t.Status == Enums.CommentStatus.Pending || t.Status == Enums.CommentStatus.Approved);
            }
            else
            {
                query = query.Where(t => t.Status == Enums.CommentStatus.Approved);
            }

            var entityList = await query.ToArrayAsync();

            return this.Transform(entityList);
        }

        public async Task<PagedResult<CommentModel>> Query(int pageIndex, int pageSize, Enums.CommentStatus? status, string keywords)
        {
            var query = this.BlogContext.Comments.AsNoTracking().AsQueryable();
            if (status.HasValue)
            {
                query = query.Where(t => t.Status == status.Value);
            }
            if (!string.IsNullOrWhiteSpace(keywords))
            {
                query = query.Where(t => t.Name.Contains(keywords) || t.Content.Contains(keywords) || t.WebSite.Contains(keywords) || t.Email.Contains(keywords));
            }

            int total = await query.CountAsync();

            query = query.OrderByDescending(t => t.Id)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize);

            var entityList = await query.ToArrayAsync();

            var modelList = this.Transform(entityList);

            return new PagedResult<CommentModel>(modelList, total);
        }

        public async Task BathUpdateStatus(int[] idList, Enums.CommentStatus status)
        {
            var entityList = await this.BlogContext.Comments.Where(t => idList.Contains(t.Id)).ToListAsync();
            foreach (var entity in entityList)
            {
                entity.Status = status;
            }

            await this.BlogContext.SaveChangesAsync();
        }

        public async Task BatchDelete(int[] idList)
        {
            int?[] deleteIdList = idList.Cast<int?>().ToArray();

            using (var tran = this.BlogContext.Database.BeginTransaction())
            {
                var entityList = await this.BlogContext.Comments.Where(t => deleteIdList.Contains(t.Id)).ToListAsync();
                this.BlogContext.RemoveRange(entityList);
                await this.BlogContext.SaveChangesAsync();

                var childReplyList = await this.BlogContext.Comments.Where(t => deleteIdList.Contains(t.ReplyToId)).ToListAsync();
                foreach (var entity in childReplyList)
                {
                    entity.ReplyToId = null;
                }
                await this.BlogContext.SaveChangesAsync();

                tran.Commit();
            }
        }

        public async Task<CommentModel> Delete(int id, bool deleteChild)
        {
            var entity = await this.BlogContext.Comments.SingleOrDefaultAsync(t => t.Id == id);
            if (entity == null)
            {
                return null;
            }

            var result = this.Transform(entity).First();

            this.BlogContext.Comments.Remove(entity);

            if (deleteChild)
            {
                var allCommentList = await this.BlogContext.Comments.Where(t => t.TopicId == entity.TopicId).ToListAsync();
                var idList = this.GetChildCommentIdList(allCommentList, entity.Id);

                var deleteEntityList = allCommentList.Where(t => idList.Contains(t.Id)).ToList();
                this.BlogContext.Comments.RemoveRange(deleteEntityList);
            }
            else
            {
                var replyEntityList = await this.BlogContext.Comments.Where(t => t.ReplyToId == entity.Id).ToListAsync();
                foreach (var replyEntity in replyEntityList)
                {
                    replyEntity.ReplyToId = null;
                }
            }

            await this.BlogContext.SaveChangesAsync();

            return result;
        }

        public async Task ApprovePendingComments(int topicId)
        {
            var entityList = await this.BlogContext.Comments.Where(t => t.TopicId == topicId && t.Status == Enums.CommentStatus.Pending).ToListAsync();

            foreach (var entity in entityList)
            {
                entity.Status = Enums.CommentStatus.Approved;
            }

            await this.BlogContext.SaveChangesAsync();
        }

        public async Task<CommentModel> ApproveComment(int id)
        {
            var entity = await this.BlogContext.Comments.SingleOrDefaultAsync(t => t.Id == id && t.Status == Enums.CommentStatus.Pending);

            if (entity == null)
            {
                return null;
            }

            entity.Status = Enums.CommentStatus.Approved;
            await this.BlogContext.SaveChangesAsync();

            return this.Transform(entity).First();
        }

        public async Task<List<CommentItemModel>> QueryLatest(int count)
        {
            string cacheKey = "Cache_Comment_Latest";
            var result = await this.Cache.RetriveCacheAsync(cacheKey, async () =>
            {
                var entityList = await this.BlogContext.Comments.AsNoTracking().Where(t => t.Status == Enums.CommentStatus.Approved)
                .OrderByDescending(t => t.Id)
                .Take(count)
                .ToListAsync();

                if (entityList.Count == 0)
                {
                    return new List<CommentItemModel>();
                }

                var topicIdList = entityList.Select(t => t.TopicId).ToArray();
                var topicList = await this.BlogContext.Topics
                    .Where(t => topicIdList.Contains(t.Id))
                    .Select(t => new TopicBasicModel
                    {
                        Id = t.Id,
                        Title = t.Title,
                        Alias = t.Alias
                    }).ToListAsync();
                var topicComments = await this.BlogContext.Comments.AsNoTracking().Where(t => topicIdList.Contains(t.TopicId))
                    .GroupBy(t => t.TopicId)
                    .Select(g => new
                    {
                        TopicId = g.Key,
                        Total = g.Count(),
                        Approved = BlogContext.Comments.Where(t => t.Status == Enums.CommentStatus.Approved).Count(),
                        Pending = BlogContext.Comments.Where(t => t.Status == Enums.CommentStatus.Pending).Count(),
                        Reject = BlogContext.Comments.Where(t => t.Status == Enums.CommentStatus.Reject).Count()
                    }).ToListAsync();

                var modelList = entityList.Select(entity =>
                {
                    var commentModel = new CommentItemModel
                    {
                        Id = entity.Id,
                        Name = entity.Name,
                        Content = entity.Content
                    };

                    var topic = topicList.SingleOrDefault(t => t.Id == entity.TopicId);
                    commentModel.Topic = new TopicBasicModel
                    {
                        Id = topic.Id,
                        Title = topic.Title,
                        Alias = topic.Alias,
                        Comments = new CommentCountModel()
                    };

                    var commentCount = topicComments.SingleOrDefault(t => t.TopicId == entity.TopicId);
                    if (commentCount != null)
                    {
                        commentModel.Topic.Comments.Approved = commentCount.Approved;
                        commentModel.Topic.Comments.Reject = commentCount.Reject;
                        commentModel.Topic.Comments.Pending = commentCount.Pending;
                        commentModel.Topic.Comments.Total = commentCount.Total;
                    }

                    return commentModel;
                });

                return modelList.ToList();
            });

            return result;
        }

        private List<CommentModel> Transform(params Comment[] entityList)
        {
            var userIdList = entityList.Where(t => t.UserId.HasValue).Select(t => t.UserId.Value).ToList();
            var userList = this.BlogContext.QueryUserFromCache().Where(t => userIdList.Contains(t.Id)).ToList();

            var result = from comment in entityList
                         join user in userList on comment.UserId equals user.Id into u
                         from user in u.DefaultIfEmpty()
                         select new CommentModel
                         {
                             Content = comment.Content,
                             CreateDate = comment.CreateDate,
                             CreateIP = comment.CreateIP,
                             Email = comment.Email,
                             Id = comment.Id,
                             Name = comment.Name,
                             ReplyToId = comment.ReplyToId,
                             Status = comment.Status,
                             TopicId = comment.TopicId,
                             WebSite = comment.WebSite,
                             User = user != null ? new CommentModel.UserModel
                             {
                                 Nickname = user.Nickname,
                                 Email = user.Email,
                                 Id = user.Id,
                                 UserName = user.UserName
                             } : null
                         };

            return result.ToList();
        }

        private List<int> GetChildCommentIdList(List<Comment> entityList, int parent)
        {
            List<int> result = new List<int>();
            result.Add(parent);

            var children = entityList.Where(t => t.ReplyToId == parent).ToList();
            foreach (var child in children)
            {
                result.AddRange(this.GetChildCommentIdList(entityList, child.Id));
            }

            return result;
        }
    }
}
