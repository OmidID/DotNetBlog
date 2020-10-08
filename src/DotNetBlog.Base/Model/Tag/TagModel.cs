using DotNetBlog.Model.Topic;

namespace DotNetBlog.Model.Tag
{
    public class TagModel
    {
        public int Id { get; set; }

        public string Keyword { get; set; }

        public TopicCountModel Topics { get; set; }
    }
}
