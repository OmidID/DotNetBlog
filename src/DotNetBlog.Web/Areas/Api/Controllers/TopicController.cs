﻿using DotNetBlog.Model.Topic;
using DotNetBlog.Service;
using DotNetBlog.Model.Api.Topic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using System.Threading.Tasks;

namespace DotNetBlog.Web.Areas.Api.Controllers
{
    [Area("Api")]
    [Route("api/topic")]
    public class TopicController : ControllerBase
    {
        private TopicService TopicService { get; set; }

        private IHtmlLocalizer<TopicController> L { get; set; }

        public TopicController(TopicService topicService, IHtmlLocalizer<TopicController> localizer)
        {
            this.TopicService = topicService;
            this.L = localizer;
        }

        [HttpPost("")]
        public async Task<IActionResult> Add([FromBody] AddTopicModel model)
        {
            if (model == null)
            {
                return InvalidRequest();
            }

            var result = await TopicService.Add(model);
            if (result.Success)
            {
                return Success(result.Data);
            }
            else
            {
                return Error(result.ErrorMessage);
            }
        }

        [HttpPost("{id:int}")]
        public async Task<IActionResult> Edit([FromRoute] int id, [FromBody] EditTopicModel model)
        {
            if (model == null)
            {
                return InvalidRequest();
            }

            model.Id = id;

            var result = await TopicService.Edit(model);
            if (result.Success)
            {
                return Success(result.Data);
            }
            else
            {
                return Error(result.ErrorMessage);
            }
        }

        [HttpGet("query")]
        public async Task<IActionResult> Query([FromQuery] QueryTopicModel model)
        {
            if (model == null)
            {
                return InvalidRequest();
            }

            var result = await TopicService.QueryNotTrash(model.PageIndex, model.PageSize, model.Status, model.Keywords);
            if (result.Success)
            {
                return PagedData(result.Data, result.Total);
            }
            else
            {
                return Error(result.ErrorMessage);
            }
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> Get(int id)
        {
            var model = await TopicService.Get(id);
            if (model == null)
            {
                return Error(L["Article does not exist"].Value);
            }
            return Success(model);
        }

        [HttpPost("batch/publish")]
        public async Task<IActionResult> BatchPublish([FromBody] BatchModel model)
        {
            if (model == null)
            {
                return InvalidRequest();
            }

            await this.TopicService.BatchUpdateStatus(model.TopicList, Enums.TopicStatus.Published);

            return Success();
        }

        [HttpPost("batch/draft")]
        public async Task<IActionResult> BatchDraft([FromBody] BatchModel model)
        {
            if (model == null)
            {
                return InvalidRequest();
            }

            await this.TopicService.BatchUpdateStatus(model.TopicList, Enums.TopicStatus.Draft);

            return Success();
        }

        [HttpPost("batch/trash")]
        public async Task<IActionResult> BatchTrash([FromBody] BatchModel model)
        {
            if (model == null)
            {
                return InvalidRequest();
            }

            await this.TopicService.BatchUpdateStatus(model.TopicList, Enums.TopicStatus.Trash);

            return Success();
        }

        [HttpGet("draft")]
        public async Task<IActionResult> QueryDraft(int count)
        {
            if (count < 1)
            {
                return this.InvalidRequest();
            }

            var result = await this.TopicService.QueryNotTrash(1, count, Enums.TopicStatus.Draft, null);

            return Success(result.Data);
        }
    }
}
