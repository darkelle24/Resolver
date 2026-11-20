using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace VMRuntime;

internal static class VmFragmentJson
{
    internal static string Serialize(JsonNode? fragment)
    {
        using var stream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(stream)) Write(writer, fragment, 0);
        return Encoding.UTF8.GetString(stream.ToArray());
    }
    private static void Number(Utf8JsonWriter writer, string number)
    {
        if (number.IndexOfAny(['.', 'e', 'E']) < 0) number += ".0";
        writer.WriteRawValue(number);
    }
    private static void Write(Utf8JsonWriter writer, JsonNode? node, int depth)
    {
        if (depth > 128) throw new ArgumentException("JSON fragment exceeds depth 128.");
        switch (node)
        {
            case null: writer.WriteNullValue(); return;
            case JsonObject obj:
                writer.WriteStartObject();
                foreach (var pair in obj)
                {
                    writer.WritePropertyName(pair.Key);
                    Write(writer, pair.Value, depth + 1);
                }
                writer.WriteEndObject();
                return;
            case JsonArray array:
                writer.WriteStartArray();
                foreach (var item in array) Write(writer, item, depth + 1);
                writer.WriteEndArray();
                return;
            case JsonValue value:
                if (value.TryGetValue<JsonElement>(out var element)) { element.WriteTo(writer); return; }
                if (value.TryGetValue<float>(out float single))
                {
                    if (!float.IsFinite(single)) throw new ArgumentException("JSON cannot contain non-finite numbers.");
                    Number(writer, single.ToString("R", CultureInfo.InvariantCulture));
                    return;
                }
                if (value.TryGetValue<double>(out double number))
                {
                    if (!double.IsFinite(number)) throw new ArgumentException("JSON cannot contain non-finite numbers.");
                    Number(writer, number.ToString("R", CultureInfo.InvariantCulture));
                    return;
                }
                value.WriteTo(writer);
                return;
            default: throw new ArgumentException("Unsupported JSON node type.");
        }
    }
}
