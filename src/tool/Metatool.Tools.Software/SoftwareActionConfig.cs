using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Metatool.Service;
using Microsoft.Extensions.Logging;

namespace Metatool.Tools.Software
{
    public class SoftwareActionConfig
    {
        public bool   Handled      { get; set; } = true;
        public string ActionId     { get; set; } = "ShortcutLaunch";
        public string Args         { get; set; }
        public bool   ShowIfOpened { get; set; } = true;

        /// <summary>
        /// regex
        /// </summary>
        public string ShowIfOpenedTitle { get; set; }

        public RunMode RunMode { get; set; } = RunMode.Inherit;

        public static SoftwareActionConfig Parse(string jsonString)
        {
            if (!jsonString.TrimStart().StartsWith('{') || string.IsNullOrEmpty(jsonString))
                return new SoftwareActionConfig();

            var options = new JsonSerializerOptions();
            options.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));

            try
            {
                return JsonSerializer.Deserialize<SoftwareActionConfig>(jsonString, options);
            }
            catch (Exception e)
            {
                Services.CommonLogger.LogError(e,
                    $"SoftwareActionConfig Parse: cannot parse SoftwareActionConfig properties + {e.Message}");
                throw;
            }
        }
    }
}