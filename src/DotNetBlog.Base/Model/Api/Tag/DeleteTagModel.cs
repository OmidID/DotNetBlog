using System.ComponentModel.DataAnnotations;

namespace DotNetBlog.Model.Api.Tag
{
    public class DeleteTagModel
    {
        [Required]
        public int[] TagList { get; set; }
    }
}
