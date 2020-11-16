using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DotNetBlog.WebAdmin
{
    public class SharedResourceManagerStringLocalizerFactory : ResourceManagerStringLocalizerFactory
    {
        private static Type SharedType = typeof(DotNetBlog.Shared);
        private static string SharedResourcePath = typeof(DotNetBlog.Resources.Shared).FullName;


        public SharedResourceManagerStringLocalizerFactory(IOptions<LocalizationOptions> localizationOptions, ILoggerFactory loggerFactory) : base(localizationOptions, loggerFactory)
        {
        }

        protected override string GetResourcePrefix(System.Reflection.TypeInfo typeInfo)
        {
            if (typeInfo == SharedType)
                return SharedResourcePath;
            return base.GetResourcePrefix(typeInfo);
        }
    }
}
