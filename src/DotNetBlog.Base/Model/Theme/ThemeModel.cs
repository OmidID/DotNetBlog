﻿using Newtonsoft.Json;

namespace DotNetBlog.Model.Theme
{
    public class ThemeModel
    {
        [JsonIgnore]
        public string Path { get; set; }

        public string Key { get; set; }

        public string Name { get; set; }
        public string Description { get; set; }
        public string ThemeUrl { get; set; }
        public string Author { get; set; }
        public string AuthorUrl { get; set; }
        public string AuthorEmail { get; set; }

    }
}
