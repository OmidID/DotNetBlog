using System.ComponentModel.DataAnnotations;

namespace DotNetBlog.Model.Api.Tag
{
    public class SaveTagModel
    {
        [Required]
        [StringLength(50)]
        public string Keyword { get; set; }
    }
}
