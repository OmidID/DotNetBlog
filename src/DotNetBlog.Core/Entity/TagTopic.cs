namespace DotNetBlog.Entity
{
    public class TagTopic
    {
        public int TagId { get; set; }

        public int TopicId { get; set; }

        public virtual Tag Tag { get; set; }

        public virtual Topic Topic { get; set; }
    }
}
