using System.ComponentModel.DataAnnotations;

namespace DotNetBlog.Model.Api.Config
{
    public class AdvanceConfigModel
    {
        [Required]
        public string ErrorPageTitle { get; set; }

        [Required]
        public string ErrorPageContent { get; set; }

        public string HeaderScript { get; set; }

        public string FooterScript { get; set; }
    }
}
