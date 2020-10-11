using System.ComponentModel.DataAnnotations;

namespace DotNetBlog.Model.Api.Comment
{
    public class ReplyCommentModel
    {
        public int ReplyTo { get; set; }

        [Required]
        public string Content { get; set; }
    }
}
