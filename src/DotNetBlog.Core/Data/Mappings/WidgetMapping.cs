using DotNetBlog.Core.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DotNetBlog.Core.Data.Mappings
{
    public sealed class WidgetMapping
    {
        public static void Map(EntityTypeBuilder<Widget> builder)
        {
            builder.ToTable("Widgets");

            builder.HasKey(t => new { t.Id });

            builder.Property(t => t.Config).IsRequired();
        }
    }
}
