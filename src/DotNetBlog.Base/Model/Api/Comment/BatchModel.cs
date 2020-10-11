using System.ComponentModel.DataAnnotations;

namespace DotNetBlog.Model.Api.Comment
{
    public class BatchModel
    {
        [Required]
        public int[] CommentList { get; set; }
    }
}
