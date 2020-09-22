using DotNetBlog.Core.Data.Mappings;
using DotNetBlog.Core.Entity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;

namespace DotNetBlog.Core.Data
{
    public class SqlServerBlogContext : BlogContext
    {
        public SqlServerBlogContext(DbContextOptions options, IServiceProvider serviceProvider)
            : base(options, serviceProvider)
        {

        }
    }
    public class PostgreeSqlBlogContext : BlogContext
    {
        public PostgreeSqlBlogContext(DbContextOptions options, IServiceProvider serviceProvider)
            : base(options, serviceProvider)
        {

        }
    }
    public class MySqlBlogContext : BlogContext
    {
        public MySqlBlogContext(DbContextOptions options, IServiceProvider serviceProvider)
            : base(options, serviceProvider)
        {

        }
    }
    public class SqliteBlogContext : BlogContext
    {
        public SqliteBlogContext(DbContextOptions options, IServiceProvider serviceProvider)
            : base(options, serviceProvider)
        {

        }
    }

    public class BlogContext : IdentityDbContext<User, UserRole, long>
    {
        public virtual DbSet<Setting> Settings { get; set; }

        public virtual DbSet<Category> Categories { get; set; }

        public virtual DbSet<CategoryTopic> CategoryTopics { get; set; }

        public virtual DbSet<Topic> Topics { get; set; }

        public virtual DbSet<Tag> Tags { get; set; }

        public virtual DbSet<TagTopic> TagTopics { get; set; }

        public virtual DbSet<Comment> Comments { get; set; }

        public virtual DbSet<Page> Pages { get; set; }

        public virtual DbSet<Widget> Widgets { get; set; }

        public IServiceProvider ServiceProvider { get; private set; }

        public BlogContext(DbContextOptions options, IServiceProvider serviceProvider)
            : base(options)
        {
            ServiceProvider = serviceProvider;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>()
                .Property(p => p.Id)
                .ValueGeneratedOnAdd()
                .UseIdentityColumn();

            modelBuilder.Entity<UserRole>()
                .Property(p => p.Id)
                .ValueGeneratedOnAdd()
                .UseIdentityColumn();

            modelBuilder.Entity<Setting>(SettingMapping.Map);
            modelBuilder.Entity<Category>(CategoryMapping.Map);
            modelBuilder.Entity<CategoryTopic>(CategoryTopicMapping.Map);
            modelBuilder.Entity<Topic>(TopicMapping.Map);
            modelBuilder.Entity<Tag>(TagMapping.Map);
            modelBuilder.Entity<TagTopic>(TagTopicMapping.Map);
            modelBuilder.Entity<Comment>(CommentMapping.Map);
            modelBuilder.Entity<Page>(PageMapping.Map);
            modelBuilder.Entity<Widget>(WidgetMapping.Map);
        }
    }
}
