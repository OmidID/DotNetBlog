using System;

namespace DotNetBlog.Model.Comment
{
    public class CommentModel
    {
        public int Id { get; set; }

        public int TopicId { get; set; }

        public int? ReplyToId { get; set; }

        public string Name { get; set; }

        public string Email { get; set; }

        public string WebSite { get; set; }

        public string Content { get; set; }

        public Enums.CommentStatus Status { get; set; }

        public DateTime CreateDate { get; set; }

        public string CreateIP { get; set; }

        public UserModel User { get; set; }

        public class UserModel
        {
            public long Id { get; set; }

            public string UserName { get; set; }

            public string Email { get; set; }

            public string Nickname { get; set; }
        }
    }
}
