namespace DotNetBlog.Entity
{
    public class CategoryTopic
    {
        public int CategoryId { get; set; }

        public int TopicId { get; set; }

        public virtual Category Category { get; set; }

        public virtual Topic Topic { get; set; }
    }
}
