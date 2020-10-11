using System.ComponentModel.DataAnnotations;

namespace DotNetBlog.Model.Api.Category
{
    public class SaveCategoryModel
    {
        [Required]
        [StringLength(50)]
        public string Name { get; set; }

        [StringLength(200)]
        public string Description { get; set; }
    }
}
