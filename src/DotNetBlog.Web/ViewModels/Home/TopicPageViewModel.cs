using DotNetBlog.Model.Comment;
using DotNetBlog.Model.Topic;
using System.Collections.Generic;

namespace DotNetBlog.Web.ViewModels.Home
{
    public class TopicPageViewModel
    {
        public bool AllowComment { get; set; }

        public CommentFormModel CommentForm { get; set; }

        public TopicModel Topic { get; set; }

        public TopicModel PrevTopic { get; set; }

        public TopicModel NextTopic { get; set; }

        public List<TopicModel> RelatedTopicList { get; set; }

        public List<CommentModel> CommentList { get; set; }
    }
}
