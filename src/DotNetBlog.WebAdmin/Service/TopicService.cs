using DotNetBlog.Model.Api;
using DotNetBlog.Enums;
using DotNetBlog.Model.Api;
using DotNetBlog.Model.Api.Topic;
using DotNetBlog.Model.Topic;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Net.Http.Json;

namespace DotNetBlog.WebAdmin.Service
{
    public class TopicService
    {
        private const string CacheKeyTopicTypes = "__TOPIC_TYPES";

        private IMemoryCache _memoryCache;

        public TopicService(Api api, IMemoryCache memoryCache)
        {
            Api = api;
            _memoryCache = memoryCache;
        }

        public Api Api { get; }

        public async Task<PagedApiResponse<TopicModel>> GetTopicsAsync(
            int page,
            int pageSize,
            TopicStatus? status,
            string keywords)
        {
            return await Api.GetAsync<QueryTopicModel, PagedApiResponse<TopicModel>>(
                $"/api/topic/query",
                new QueryTopicModel
                {
                    PageIndex = page,
                    PageSize = pageSize,
                    Status = status,
                    Keywords = keywords
                });
        }

    }
}