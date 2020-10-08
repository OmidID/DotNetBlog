using System;

namespace DotNetBlog.Model.Page
{
    public class PageBasicModel
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public string Alias { get; set; }

        public PageBasicModel Parent { get; set; }

        public int Order { get; set; }

        public DateTime Date { get; set; }

        public bool IsHomePage { get; set; }

        public bool ShowInList { get; set; }

        public Enums.PageStatus Status { get; set; }
    }
}
