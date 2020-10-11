using System.ComponentModel.DataAnnotations;

namespace DotNetBlog.Model.Api.Page
{
    public class BatchModel
    {
        [Required]
        public int[] PageList { get; set; }
    }
}
