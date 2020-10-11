using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNetBlog.WebAdmin.Components
{
    public class JQueryCallingParameter
    {
        private List<string> _commands = new List<string>();
        private int _lastMethodIndex = 0;
        private IJSRuntime _jsRuntime;

        internal JQueryCallingParameter(IJSRuntime jsRuntime, string selector)
        {
            _jsRuntime = jsRuntime;
            _commands.Add(selector);
        }

        public JQueryCallingParameter Call(string method, params string[] args)
        {
            _lastMethodIndex = _commands.Count;
            _commands.Add($"--{method}");
            if (args != null)
                foreach (var arg in args)
                    _commands.Add(arg);

            return this;
        }
        
        public async Task InvokeVoidAsync()
        {
            var arguments = _commands.ToArray();

            await _jsRuntime.InvokeVoidAsync("jq", arguments);
        }

    }
}
