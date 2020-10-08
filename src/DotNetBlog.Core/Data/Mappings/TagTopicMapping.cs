using DotNetBlog.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DotNetBlog.Data.Mappings
{
    public static class TagTopicMapping
    {
        public static void Map(EntityTypeBuilder<TagTopic> builder)
        {
            builder.ToTable("TagTopics");

            builder.HasKey(t => new { t.TagId, t.TopicId });
            builder.HasOne(t => t.Topic).WithMany();
            builder.HasOne(t => t.Tag).WithMany();
        }
    }
}
