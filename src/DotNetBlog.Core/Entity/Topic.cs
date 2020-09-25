using DotNetBlog.Core.Enums;
using System;

namespace DotNetBlog.Core.Entity
{
    public class Topic
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public string Content { get; set; }

        public string Alias { get; set; }

        public string Summary { get; set; }

        public bool AllowComment { get; set; }

        public TopicStatus Status { get; set; }

        public long CreateUserId { get; set; }

        public DateTime CreateDate { get; set; }

        public long EditUserId { get; set; }

        public DateTime EditDate { get; set; }
    }
}
