using Newtonsoft.Json.Linq;
using System.ComponentModel.DataAnnotations;

namespace DotNetBlog.Model.Api.Widget
{
    public class SaveWidgetModel
    {
        public Enums.WidgetType Type { get; set; }

        [Required]
        public JObject Config { get; set; }
    }
}
