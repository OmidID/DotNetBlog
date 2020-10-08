namespace DotNetBlog.Model.Comment
{
    public class CommentItemModel
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Content { get; set; }

        public Topic.TopicBasicModel Topic { get; set; }
    }
}
