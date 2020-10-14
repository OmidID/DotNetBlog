using DotNetBlog;
using DotNetBlog.Model.Comment;
using DotNetBlog.Model.Page;
using DotNetBlog.Model.Setting;
using DotNetBlog.Model.Topic;
using DotNetBlog.Service;
using DotNetBlog.Web.ViewModels.Home;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Localization;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DotNetBlog.Web.Controllers
{
    [Filters.ErrorHandleFilter]
    public class HomeController : Controller
    {
        private const string CookieCommentName = "DotNetBlog_CommentName";

        private const string CookieCommentEmail = "DotNetBlog_CommentEmail";

        private TopicService TopicService { get; set; }
        private SettingModel SettingModel { get; set; }
        private IStringLocalizer<Shared> L { get; set; }

        public HomeController(
            TopicService topicService,
            CategoryService categoryService,
            SettingModel settingModel,
            TagService tagService,
            CommentService commentService,
            PageService pageService,
            ClientManager clientManager,
            IStringLocalizer<Shared> localizer)
        {
            TopicService = topicService;
            SettingModel = settingModel;
            L = localizer;
        }

        [Route("{page:int?}")]
        public async Task<IActionResult> Index([FromServices] InstallService installService, int page = 1)
        {
            if (installService.NeedToInstall())
                return RedirectToAction("Index", "Install");

            var pageSize = SettingModel.TopicsPerPage;
            var topicList = await TopicService.QueryNotTrash(page, pageSize, Enums.TopicStatus.Published, null);

            var vm = new IndexPageViewModel
            {
                TopicList = new TopicListModel
                {
                    Data = topicList.Data,
                    PageIndex = page,
                    PageSize = pageSize,
                    Total = topicList.Total
                }
            };

            return View(vm);
        }

        [HttpGet("category/{id:int}/{page:int?}")]
        public async Task<IActionResult> Category([FromServices] CategoryService categoryService, int id, int page = 1)
        {
            var category = (await categoryService.All()).SingleOrDefault(t => t.Id == id);

            if (category == null)
            {
                return this.NotFound();
            }

            ViewBag.Title = L["Category: {0}", category.Name];

            int pageSize = SettingModel.TopicsPerPage;
            var topicList = await TopicService.QueryByCategory(page, pageSize, category.Id);

            var vm = new CategoryPageViewModel
            {
                Category = category,
                TopicList = new TopicListModel
                {
                    Data = topicList.Data,
                    PageIndex = page,
                    PageSize = pageSize,
                    Total = topicList.Total
                }
            };

            return View(vm);
        }

        [HttpGet("tag/{keyword}/{page:int?}")]
        public async Task<IActionResult> Tag(string keyword, int page = 1)
        {
            ViewBag.Title = L["Tag: {0}", keyword];

            int pageSize = SettingModel.TopicsPerPage;

            var topicList = await TopicService.QueryByTag(page, pageSize, keyword);

            var vm = new TagPageViewModel
            {
                Tag = keyword,
                TopicList = new TopicListModel
                {
                    Data = topicList.Data,
                    PageIndex = page,
                    PageSize = pageSize,
                    Total = topicList.Total
                }
            };

            return View(vm);
        }

        [HttpGet("{year:int}-{month:int}/{page:int?}")]
        public async Task<IActionResult> Month(int year, int month, int page = 1)
        {
            ViewBag.Title = L["{0} year(s) and {1} month(s)", year, month];

            int pageSize = SettingModel.TopicsPerPage;

            var topicList = await TopicService.QueryByMonth(page, pageSize, year, month);

            var vm = new MonthPageViewModel
            {
                Month = month,
                Year = year,
                TopicList = new TopicListModel
                {
                    Data = topicList.Data,
                    PageIndex = page,
                    PageSize = pageSize,
                    Total = topicList.Total
                }
            };

            return View(vm);
        }

        [HttpGet("topic/{id:int}")]
        public async Task<IActionResult> Topic(int id,
            [FromServices] ClientManager clientManager,
            [FromServices] CommentService commentService)
        {
            var topic = await TopicService.Get(id);

            return await this.TopicView(topic, clientManager, commentService);
        }

        [HttpGet("topic/{alias}")]
        public async Task<IActionResult> TopicByAlias(string alias,
            [FromServices] ClientManager clientManager,
            [FromServices] CommentService commentService)
        {
            var topic = await TopicService.Get(alias);

            return await this.TopicView(topic, clientManager, commentService);
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search(string keywords, int page = 1)
        {
            SearchPageViewModel vm = new SearchPageViewModel
            {
                Keywords = keywords,
                TopicList = new TopicListModel
                {
                    Data = new List<TopicModel>()
                }
            };

            int pageSize = 10;

            ViewBag.Title = L["Search result: {0}", keywords];

            if (!string.IsNullOrWhiteSpace(keywords))
            {
                var topicList = await TopicService.QueryByKeywords(page, pageSize, keywords);

                vm.TopicList = new TopicListModel
                {
                    Data = topicList.Data,
                    PageIndex = page,
                    PageSize = pageSize,
                    Total = topicList.Total
                };
            }

            return this.View(vm);
        }

        [HttpGet("page/{id:int}")]
        public async Task<IActionResult> Page(int id,
            [FromServices] PageService pageService,
            [FromServices] ClientManager clientManager)
        {
            var page = await pageService.Get(id);

            return this.PageView(page, clientManager);
        }

        [HttpGet("page/{alias}")]
        public async Task<IActionResult> PageByAlias(string alias,
            [FromServices] PageService pageService,
            [FromServices] ClientManager clientManager)
        {
            var page = await pageService.Get(alias);

            return this.PageView(page, clientManager);
        }

        [NonAction]
        private async Task<IActionResult> TopicView(TopicModel topic, ClientManager clientManager, CommentService commentService)
        {
            if (topic == null)
            {
                return NotFound();
            }
            if (topic.Status != Enums.TopicStatus.Published && !clientManager.IsLogin)
            {
                return NotFound();
            }

            ViewBag.Title = topic.Title;

            var prevTopic = await TopicService.GetPrev(topic);
            var nextTopic = await TopicService.GetNext(topic);
            var relatedTopicList = await TopicService.QueryRelated(topic);
            var commentList = await commentService.QueryByTopic(topic.Id);

            var vm = new TopicPageViewModel
            {
                AllowComment = this.TopicService.CanComment(topic),
                Topic = topic,
                PrevTopic = prevTopic,
                NextTopic = nextTopic,
                RelatedTopicList = relatedTopicList,
                CommentList = commentList,
                CommentForm = new CommentFormModel()
            };

            vm.CommentForm.Name = Request.Cookies[CookieCommentName];
            vm.CommentForm.Email = Request.Cookies[CookieCommentEmail];

            return View("Topic", vm);
        }

        [NonAction]
        private IActionResult PageView(PageModel page, ClientManager clientManager)
        {
            if (page == null)
            {
                return NotFound();
            }
            if (page.Status != Enums.PageStatus.Published && !clientManager.IsLogin)
            {
                return NotFound();
            }

            ViewBag.Title = page.Title;

            return View("Page", page);
        }

        [HttpPost("comment/add")]
        public async Task<IActionResult> AddComment([FromForm] AddCommentModel model,
            [FromServices] CommentService commentService)
        {
            if (model == null || !ModelState.IsValid)
            {
                return this.Notice(new NoticePageViewModel
                {
                    Message = L["Invalid request, please try again later"].Value,
                    RedirectUrl = Url.Action("Topic", "Home", new { id = model.TopicId }),
                    MessageType = NoticePageViewModel.NoticeMessageType.Error
                });
            }

            var result = await commentService.Add(model);

            this.Response.Cookies.Append(CookieCommentName, model.Name);
            this.Response.Cookies.Append(CookieCommentEmail, model.Email);

            if (result.Success)
            {
                if (result.Data.Status != Enums.CommentStatus.Approved)
                {
                    return this.Notice(new NoticePageViewModel
                    {
                        Message = L["Your comment has been added successfully and requires an administrator to approve it before it can be displayed"].Value,
                        RedirectUrl = Url.Action("Topic", "Home", new { id = model.TopicId }),
                        MessageType = NoticePageViewModel.NoticeMessageType.Success
                    });
                }
            }
            else
            {
                return this.Notice(new NoticePageViewModel
                {
                    Message = result.ErrorMessage,
                    RedirectUrl = Url.Action("Topic", "Home", new { id = model.TopicId }),
                    MessageType = NoticePageViewModel.NoticeMessageType.Error
                });
            }

            return this.RedirectToAction("Topic", "Home", new { id = result.Data.TopicId });
        }

        [NonAction]
        private IActionResult Notice(NoticePageViewModel vm)
        {
            return this.View("Notice", vm);
        }
    }
}
