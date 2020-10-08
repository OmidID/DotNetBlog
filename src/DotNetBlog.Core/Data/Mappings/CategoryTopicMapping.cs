using DotNetBlog.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DotNetBlog.Data.Mappings
{
    public static class CategoryTopicMapping
    {
        public static void Map(EntityTypeBuilder<CategoryTopic> builder)
        {
            builder.ToTable("CategoryTopics");

            builder.HasKey(t => new { t.CategoryId, t.TopicId });
            builder.HasOne(t => t.Category).WithMany();
            builder.HasOne(t => t.Topic).WithMany();
        }
    }
}
