﻿using DotNetBlog.Core.Enums;
using System;

namespace DotNetBlog.Core.Entity
{
    public class Page
    {
        public int Id { get; set; }

        public int? ParentId { get; set; }

        public string Title { get; set; }

        public string Content { get; set; }

        public string Alias { get; set; }

        public string Keywords { get; set; }

        public string Description { get; set; }

        public DateTime CreateDate { get; set; }

        public DateTime EditDate { get; set; }

        public bool IsHomePage { get; set; }

        public bool ShowInList { get; set; }

        public int Order { get; set; }

        public PageStatus Status { get; set; }
    }
}
