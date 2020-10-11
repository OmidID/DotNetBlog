using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetBlog.WebAdmin.Components
{
    public static class JsRuntimeExtentions
    {

        public static JQueryCallingParameter JQuery(
            this IJSRuntime jsRuntime, 
            string selector)
        {
            return new JQueryCallingParameter(jsRuntime, selector);
        }

    }
}
