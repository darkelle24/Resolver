using System.Text.Json;

namespace VMRuntime.Diagnostics;

/// <summary>An immutable, bounded JSON snapshot; host objects are represented by type name only.</summary>
public sealed class VmValueSnapshot : VmJsonObject
{
    /// <summary>VM type, or unavailable when no value was produced.</summary>
    public string Type { get; }
    /// <summary>Detached JSON payload, safe to keep after mutable inputs change.</summary>
    public JsonElement Value { get; }
    /// <summary>Whether collection depth/size or string limits omitted data.</summary>
    public bool Truncated { get; }
    private VmValueSnapshot(string type, object? value, bool truncated = false)
        => (Type, Value, Truncated) = (type, JsonSerializer.SerializeToElement(value, VmJson.Options(false)), truncated);

    /// <summary>Creates an explicit marker for an input that was not available.</summary>
    public static VmValueSnapshot Unavailable(string reason) => new("unavailable", reason);
    /// <summary>Captures a value without retaining host references or mutable VM collections.</summary>
    public static VmValueSnapshot Capture(DynValue value)
    {
        bool truncated = false;
        int remaining = 4096;
        var seen = new HashSet<object>(ReferenceEqualityComparer.Instance);
        object? payload = CapturePayload(value, 0, seen, ref truncated, ref remaining);
        return new(value.Type.ToString().ToLowerInvariant(), payload, truncated);
    }
    internal static VmValueSnapshot Literal(string value) => new("string", value);

    private static object? CapturePayload(DynValue value, int depth, HashSet<object> seen, ref bool truncated, ref int remaining)
    {
        if (remaining-- <= 0) { truncated = true; return "<snapshot limit>"; }
        if (depth > 8) { truncated = true; return "<depth limit>"; }
        switch (value.Type)
        {
            case ValueType.Null: return null;
            case ValueType.Int: return value.AsInt();
            case ValueType.Float: return value.AsFloat();
            case ValueType.Bool: return value.AsBool();
            case ValueType.String:
                string text = value.AsString();
                if (text.Length > 4096) { truncated = true; return text[..4096]; }
                return text;
            case ValueType.Vector2: var v2 = value.AsVector2(); return new { v2.X, v2.Y };
            case ValueType.Vector2i: var i2 = value.AsVector2i(); return new { i2.X, i2.Y };
            case ValueType.Vector3: var v3 = value.AsVector3(); return new { v3.X, v3.Y, v3.Z };
            case ValueType.Vector3i: var i3 = value.AsVector3i(); return new { i3.X, i3.Y, i3.Z };
            case ValueType.Color: var c = value.AsColor(); return new { c.R, c.G, c.B, c.A };
            case ValueType.Object: return new { ClrType = value.AsObject().GetType().FullName };
            case ValueType.Array:
            {
                var list = value.AsArray();
                if (!seen.Add(list)) { truncated = true; return "<cycle>"; }
                var result = new List<object?>();
                foreach (var item in list.Take(128))
                {
                    if (remaining <= 0) { truncated = true; break; }
                    result.Add(CapturePayload(item, depth + 1, seen, ref truncated, ref remaining));
                }
                if (list.Count > 128) truncated = true;
                seen.Remove(list);
                return result;
            }
            case ValueType.Dictionary:
            {
                var dict = value.AsDictionary();
                if (!seen.Add(dict)) { truncated = true; return "<cycle>"; }
                var result = new List<object>();
                foreach (var pair in dict.Take(128))
                {
                    if (remaining <= 0) { truncated = true; break; }
                    result.Add(new { Key = CapturePayload(pair.Key, depth + 1, seen, ref truncated, ref remaining),
                        Value = CapturePayload(pair.Value, depth + 1, seen, ref truncated, ref remaining) });
                }
                if (dict.Count > 128) truncated = true;
                seen.Remove(dict);
                return result;
            }
            default: return "<unsupported>";
        }
    }
}
