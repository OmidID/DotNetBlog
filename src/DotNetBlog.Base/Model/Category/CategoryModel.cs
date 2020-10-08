using DotNetBlog.Model.Topic;

namespace DotNetBlog.Model.Category
{
    public class CategoryModel : CategoryBasicModel
    {
        public string Description { get; set; }

        public TopicCountModel Topics { get; set; }
    }
}
