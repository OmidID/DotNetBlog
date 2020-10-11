using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetBlog.WebAdmin.Components
{
    public class GlobalEventArgs<T> : EventArgs
    {

        public T Data { get; set; }

    }
}
