using DotNetBlog.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DotNetBlog.Data.Mappings
{
    public static class TopicMapping
    {
        public static void Map(EntityTypeBuilder<Topic> builder)
        {
            builder.ToTable("Topics");

            builder.HasKey(t => t.Id);

            builder.HasIndex(t => t.Alias).IsUnique();

            builder.Property(t => t.Id).ValueGeneratedOnAdd().UseIdentityColumn();
            builder.Property(t => t.Title).IsRequired().HasMaxLength(100);
            builder.Property(t => t.Alias).HasMaxLength(100);
            builder.Property(t => t.Summary).HasMaxLength(200);
        }
    }
}
