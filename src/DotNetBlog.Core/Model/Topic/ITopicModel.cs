namespace DotNetBlog.Core.Model.Topic
{
    public interface ITopicModel
    {
        int Id { get; }

        string Title { get; }

        string Alias { get; }
    }
}
