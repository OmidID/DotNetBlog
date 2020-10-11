namespace DotNetBlog.Model.Api.Comment
{
    public class QueryCommentModel
    {
        public int PageIndex { get; set; }

        public int PageSize { get; set; }

        public Enums.CommentStatus? Status { get; set; }

        public string Keywords { get; set; }
    }
}
