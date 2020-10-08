using DotNetBlog.Enums;

namespace DotNetBlog.Model.Widget
{
    public class WidgetModel
    {
        public WidgetType Type { get; set; }

        public WidgetConfigModelBase Config { get; set; }
    }
}
