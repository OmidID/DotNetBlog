using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace DotNetBlog.WebAdmin.Components
{
    public class TagifyEventArgs : EventArgs
    {

        public ReadOnlyCollection<string> Tags { get; set; }

    }
}
