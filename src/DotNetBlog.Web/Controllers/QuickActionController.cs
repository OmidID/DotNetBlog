using DotNetBlog.Core.Service;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace DotNetBlog.Web.Controllers
{
    [Filters.ErrorHandleFilter]
    [Filters.RequireLoginFilter]
    public class QuickActionController : Controller
    {
        private CommentService CommentService { get; set; }

        private TopicService TopicService { get; set; }

        public QuickActionController(CommentService commentService, TopicService topicService)
        {
            this.CommentService = commentService;
            this.TopicService = topicService;
        }

        [HttpGet("comment/delete")]
        public async Task<IActionResult> DeleteComment(int id, bool deleteChild)
        {
            var commentModel = await this.CommentService.Delete(id, deleteChild);
            if (commentModel == null)
            {
                return this.NotFound();
            }

            return this.RedirectToAction("Topic", "Home", new { id = commentModel.TopicId });
        }

        [HttpGet("topic/{topicId:int}/approvecomments")]
        public async Task<IActionResult> ApproveComments(int topicId)
        {
            await this.CommentService.ApprovePendingComments(topicId);

            return this.RedirectToAction("Topic", "Home", new { id = topicId });
        }

        [HttpGet("comment/{commentId:int}/approve")]
        public async Task<IActionResult> ApproveComment(int commentId)
        {
            var comment = await this.CommentService.ApproveComment(commentId);

            if (comment == null)
            {
                return NotFound();
            }

            return this.RedirectToAction("Topic", "Home", new { id = comment.TopicId });
        }

        [HttpGet("topic/{topicId:int}/delete")]
        public async Task<IActionResult> DeleteTopic(int topicId)
        {
            await this.TopicService.BatchUpdateStatus(new int[] { topicId }, Core.Enums.TopicStatus.Trash);

            return this.RedirectToAction("Index", "Home");
        }
    }
}
