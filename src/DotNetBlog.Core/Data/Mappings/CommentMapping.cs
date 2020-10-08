using DotNetBlog.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DotNetBlog.Data.Mappings
{
    public static class CommentMapping
    {
        public static void Map(EntityTypeBuilder<Comment> builder)
        {
            builder.ToTable("Comments");

            builder.HasKey(t => t.Id);

            builder.Property(t => t.Id).ValueGeneratedOnAdd().UseIdentityColumn();

            builder.Property(t => t.Content).IsRequired();
            builder.Property(t => t.CreateIP).IsRequired().HasMaxLength(40);
            builder.Property(t => t.Email).IsRequired().HasMaxLength(100);
            builder.Property(t => t.Name).IsRequired().HasMaxLength(20);
            builder.Property(t => t.WebSite).HasMaxLength(100);
        }
    }
}
