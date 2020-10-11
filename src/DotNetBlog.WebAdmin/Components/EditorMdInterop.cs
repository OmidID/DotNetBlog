using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DotNetBlog.WebAdmin.Components
{
    public class EditorMdInterop
    {
        private static Dictionary<string, EditorMd> _editors = new Dictionary<string, EditorMd>();

        internal static void Initialize(EditorMd editor)
        {
            _editors[editor.InternalId] = editor;
            
        }

        internal static void Close(EditorMd editor)
        {
            if (_editors.ContainsKey(editor.InternalId))
                _editors.Remove(editor.InternalId);
        }

        [JSInvokable]
        public static Task<string> GetText(string id)
        {
            if (_editors.TryGetValue(id, out var editor))
                return Task.FromResult(editor.EditorContent);

            return Task.FromResult(string.Empty);
        }

        [JSInvokable]
        public static async Task<bool> UpdateText(string id, string editorText)
        {
            if (_editors.TryGetValue(id, out var editor))
            {
                await editor.EditorUpdated(editorText);
                return true;
            }

            return false;
        }
    }
}
