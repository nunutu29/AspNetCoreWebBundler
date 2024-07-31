using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AspNetCoreWebBundler;

internal class BundleUglifierSettingsJsonConverter : JsonConverter<BundleUglifierSettings>
{
    /// <inheritdoc />
    public override BundleUglifierSettings ReadJson(JsonReader reader, Type objectType, BundleUglifierSettings existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        var options = existingValue ?? new BundleUglifierSettings();
        var obj = JObject.Load(reader);

        foreach (var property in obj.Properties())
        {
            var key = property.Name;
            var token = property.Value;

            switch (token.Type)
            {
                case JTokenType.String:
                    options[key] = token.ToString();
                    break;
                //case JTokenType.Integer:
                //    options[key] = token.ToObject<int>();
                //    break;
                case JTokenType.Boolean:
                    options[key] = token.ToObject<bool>();
                    break;
            }

            // ingore everything else
        }

        return options;
    }
    
    public override void WriteJson(JsonWriter writer, BundleUglifierSettings value, JsonSerializer serializer)
    {
        // ignore
    }

    public sealed override bool CanWrite => false;
}