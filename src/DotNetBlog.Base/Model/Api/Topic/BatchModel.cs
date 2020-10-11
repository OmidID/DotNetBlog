using System.ComponentModel.DataAnnotations;

namespace DotNetBlog.Model.Api.Topic
{
    public class BatchModel
    {
        [Required]
        public int[] TopicList { get; set; }
    }
}
