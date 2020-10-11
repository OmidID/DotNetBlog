using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DotNetBlog.WebAdmin.Components
{
    public class TagifyInterop
    {
        private static Dictionary<string, Tagify> _tagifies = new Dictionary<string, Tagify>();

        internal static void Initialize(Tagify tagify)
        {
            _tagifies[tagify.InternalId] = tagify;

        }

        internal static void Close(Tagify tagify)
        {
            if (_tagifies.ContainsKey(tagify.InternalId))
                _tagifies.Remove(tagify.InternalId);
        }

        [JSInvokable]
        public static Task<List<string>> GetTags(string id)
        {
            if (_tagifies.TryGetValue(id, out var tagify))
                return Task.FromResult(tagify.Tags);

            return Task.FromResult(default(List<string>));
        }

        [JSInvokable]
        public static async Task<bool> TagifyAdded(string id, string tag)
        {
            if (_tagifies.TryGetValue(id, out var tagify))
            {
                await tagify.AddItem(tag);
                return true;
            }

            return false;
        }

        [JSInvokable]
        public static async Task<bool> TagifyRemoved(string id, string tag)
        {
            if (_tagifies.TryGetValue(id, out var tagify))
            {
                await tagify.RemoveItem(tag);
                return true;
            }

            return false;
        }
    }
}
