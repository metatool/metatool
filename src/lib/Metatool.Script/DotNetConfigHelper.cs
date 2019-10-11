using System.Xml.Linq;
using Newtonsoft.Json.Linq;

namespace Metatool.Script
{
    internal static class DotNetConfigHelper
    {
        private static readonly XNamespace AsmNs = "urn:schemas-microsoft-com:asm.v1";

        public static JObject CreateNetCoreRuntimeOptions()
        {
            return new JObject(
                new JProperty("runtimeOptions", new JObject(
                    new JProperty("tfm", "netcoreapp3.0"),
                    new JProperty("framework", new JObject(
                        new JProperty("name", "Microsoft.WindowsDesktop.App"),
                        new JProperty("version", "3.0.0"))))));
        }

        public static JObject CreateNetCoreDevRuntimeOptions(string packageFolder)
        {
            return new JObject(
                new JProperty("runtimeOptions", new JObject(
                    new JProperty("additionalProbingPaths", new JArray(packageFolder)))));
        }
    }
}
