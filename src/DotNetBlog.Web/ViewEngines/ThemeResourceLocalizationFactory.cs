using System;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DotNetBlog.Web.ViewEngines
{
    public class ThemeResourceLocalizationFactory : ResourceManagerStringLocalizerFactory
    {
        private static string BaseThemeName = $"{nameof(DotNetBlog)}.{nameof(DotNetBlog.Web)}.Themes.";
        private static string BaseViewName = $"{nameof(DotNetBlog)}.{nameof(DotNetBlog.Web)}.Views.";

        private static Type SharedType = typeof (Shared);
        private static string SharedResourcePath = typeof (DotNetBlog.Resources.Shared).FullName;

        public ThemeResourceLocalizationFactory(IOptions<LocalizationOptions> localizationOptions, ILoggerFactory loggerFactory) : base(localizationOptions, loggerFactory)
        {
        }

        protected override string GetResourcePrefix(string baseResourceName, string baseNamespace)
        {
            if (baseResourceName.StartsWith(BaseThemeName))
                baseResourceName = BaseViewName + baseResourceName.Substring(baseResourceName.IndexOf(".", BaseThemeName.Length) + 1);
            return base.GetResourcePrefix(baseResourceName, baseNamespace);
        }

         protected override string GetResourcePrefix(System.Reflection.TypeInfo typeInfo)
         {
            if (typeInfo == SharedType)
                return SharedResourcePath;
            return base.GetResourcePrefix(typeInfo);
         }
    }
}
