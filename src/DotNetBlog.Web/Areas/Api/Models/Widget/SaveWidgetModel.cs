using Newtonsoft.Json.Linq;
using System.ComponentModel.DataAnnotations;

namespace DotNetBlog.Web.Areas.Api.Models.Widget
{
    public class SaveWidgetModel
    {
        public Enums.WidgetType Type { get; set; }

        [Required]
        public JObject Config { get; set; }
    }
}
