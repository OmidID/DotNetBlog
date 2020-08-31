using System;

namespace DotNetBlog.Core.Entity
{
    public class Comment
    {
        public int Id { get; set; }

        public int TopicId { get; set; }

        public int? ReplyToId { get; set; }

        public Enums.CommentStatus Status { get; set; }

        public string Email { get; set; }

        public string Name { get; set; }

        public string WebSite { get; set; }

        public string Content { get; set; }

        public DateTime CreateDate { get; set; }

        public string CreateIP { get; set; }

        public bool NotifyOnComment { get; set; }

        public long? UserId { get; set; }
    }
}
