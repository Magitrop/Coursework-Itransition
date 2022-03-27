using Microsoft.Extensions.Localization;
using System.Reflection;

namespace RazorCoursework
{
    public class LocService
    {
        private readonly IStringLocalizer _localizer;
        public static bool isEnglishVersion { get; set; }

        public LocService(IStringLocalizerFactory factory)
        {
            var type = typeof(SharedResource);
            var assemblyName = new AssemblyName(type.GetTypeInfo().Assembly.FullName);
            _localizer = factory.Create("SharedResource", assemblyName.Name);
        }

        public LocalizedString GetLocalizedHtmlString(string key)
        {
            return _localizer[key];
        }
    }
}
