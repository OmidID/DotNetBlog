using DotNetBlog.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DotNetBlog.Data.Mappings
{
    public static class TagMapping
    {
        public static void Map(EntityTypeBuilder<Tag> builder)
        {
            builder.ToTable("Tags");

            builder.HasIndex(t => t.Keyword).IsUnique();

            builder.HasKey(t => t.Id);
            builder.Property(t => t.Id).ValueGeneratedOnAdd().UseIdentityColumn();
            builder.Property(t => t.Keyword).IsRequired().HasMaxLength(100);
        }
    }
}
