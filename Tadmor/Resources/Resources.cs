using System.IO;
using System.Reflection;

namespace Tadmor.Resources
{
    public static class Resource
    {
        public static Stream Load(string resourceName)
        {
            return Assembly.GetExecutingAssembly().GetManifestResourceStream($"Tadmor.Resources.{resourceName}");
        }
    }
}