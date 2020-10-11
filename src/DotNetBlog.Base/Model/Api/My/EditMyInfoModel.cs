using System.ComponentModel.DataAnnotations;

namespace DotNetBlog.Model.Api.My
{
    public class EditMyInfoModel
    {
        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; }

        [Required]
        [StringLength(20)]
        public string Nickname { get; set; }
    }
}
