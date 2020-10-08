using DotNetBlog.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DotNetBlog.Data.Mappings
{
    public sealed class WidgetMapping
    {
        public static void Map(EntityTypeBuilder<Widget> builder)
        {
            builder.ToTable("Widgets");

            builder.HasKey(t => t.Id);

            builder.Property(t => t.Id).ValueGeneratedNever();

            builder.Property(t => t.Config).IsRequired();
        }
    }
}
