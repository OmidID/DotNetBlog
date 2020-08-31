﻿namespace DotNetBlog.Core.Model.Topic
{
    public class TopicBasicModel : ITopicModel
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public string Alias { get; set; }

        public Comment.CommentCountModel Comments { get; set; }
    }
}
