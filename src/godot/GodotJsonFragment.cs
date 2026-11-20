#nullable enable
using System;
using System.Text.Json.Nodes;
using Godot;
namespace VMRuntime.GodotApi;

internal static class GodotJsonFragment
{
    internal static JsonNode? Convert(Variant value, int depth = 0)
    {
        if (depth > 128) throw new ArgumentException("JSON fragment exceeds depth 128; a collection cycle may be present.");
        switch (value.VariantType)
        {
            case Variant.Type.Nil: return null;
            case Variant.Type.Bool: return JsonValue.Create(value.AsBool());
            case Variant.Type.Int: return JsonValue.Create(value.AsInt64());
            case Variant.Type.Float: return JsonValue.Create(value.AsDouble());
            case Variant.Type.String: return JsonValue.Create(value.AsString());
            case Variant.Type.StringName: return JsonValue.Create(value.AsStringName().ToString());
            case Variant.Type.Array:
            {
                var result = new JsonArray();
                foreach (var item in value.AsGodotArray()) result.Add(Convert(item, depth + 1));
                return result;
            }
            case Variant.Type.Dictionary:
            {
                var result = new JsonObject();
                foreach (var pair in value.AsGodotDictionary())
                {
                    string key = pair.Key.VariantType switch
                    {
                        Variant.Type.String => pair.Key.AsString(),
                        Variant.Type.StringName => pair.Key.AsStringName().ToString(),
                        _ => throw new ArgumentException("JSON object keys must be strings or StringNames.")
                    };
                    if (result.ContainsKey(key)) throw new ArgumentException($"Duplicate normalized JSON key '{key}'.");
                    result.Add(key, Convert(pair.Value, depth + 1));
                }
                return result;
            }
            default: throw new ArgumentException($"JSON fragments cannot contain Godot {value.VariantType} values.");
        }
    }
}
