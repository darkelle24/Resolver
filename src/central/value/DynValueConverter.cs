#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace VMRuntime;

/// <summary>Explicit boundary between Godot Variant values and the engine-independent core.</summary>
public static class DynValueConverter
{
    /// <summary>Converts a VM value to Godot; unsupported host references and cyclic/deep collections are rejected.</summary>
    public static Variant ToVariant(DynValue value) => ToVariant(value, 0);
    private static Variant ToVariant(DynValue value, int depth)
    {
        if (depth > 64) throw new InvalidOperationException("Collection conversion exceeds depth 64; a cycle may be present.");
        switch (value.Type)
        {
            case ValueType.Null: return default;
            case ValueType.Int: return value.AsInt();
            case ValueType.Float: return value.AsFloat();
            case ValueType.Bool: return value.AsBool();
            case ValueType.String: return value.AsString();
            case ValueType.Vector2: return value.AsVector2().ToGodot();
            case ValueType.Vector2i: return value.AsVector2i().ToGodot();
            case ValueType.Vector3: return value.AsVector3().ToGodot();
            case ValueType.Vector3i: return value.AsVector3i().ToGodot();
            case ValueType.Color: return value.AsColor().ToGodot();
            case ValueType.Object: return value.AsObject<GodotObject>();
            case ValueType.Array:
                var array = new Godot.Collections.Array();
                foreach (var item in value.AsArray()) array.Add(ToVariant(item, depth + 1));
                return array;
            case ValueType.Dictionary:
                var dict = new Godot.Collections.Dictionary();
                foreach (var pair in value.AsDictionary())
                    dict[ToVariant(pair.Key, depth + 1)] = ToVariant(pair.Value, depth + 1);
                return dict;
            default: throw new NotSupportedException($"Cannot convert {value.Type} to Variant.");
        }
    }
    /// <summary>Converts a Godot value to the core; Int32 overflow and unsupported types are explicit errors.</summary>
    public static DynValue FromVariant(Variant value) => FromVariant(value, 0);
    private static DynValue FromVariant(Variant value, int depth)
    {
        if (depth > 64) throw new InvalidOperationException("Collection conversion exceeds depth 64; a cycle may be present.");
        switch (value.VariantType)
        {
            case Variant.Type.Nil: return DynValue.Null;
            case Variant.Type.Bool: return DynValue.FromBool(value.AsBool());
            case Variant.Type.Int: return DynValue.FromInt(checked((int)value.AsInt64()));
            case Variant.Type.Float:
                double number = value.AsDouble();
                float single = (float)number;
                if (!double.IsFinite(number) || !float.IsFinite(single)) throw new OverflowException("VM float must be finite Float32.");
                return DynValue.FromFloat(single);
            case Variant.Type.String: return DynValue.FromString(value.AsString());
            case Variant.Type.StringName: return DynValue.FromString(value.AsStringName().ToString());
            case Variant.Type.Vector2: return DynValue.FromVector2(value.AsVector2().ToScript());
            case Variant.Type.Vector2I: return DynValue.FromVector2i(value.AsVector2I().ToScript());
            case Variant.Type.Vector3: return DynValue.FromVector3(value.AsVector3().ToScript());
            case Variant.Type.Vector3I: return DynValue.FromVector3i(value.AsVector3I().ToScript());
            case Variant.Type.Color: return DynValue.FromColor(value.AsColor().ToScript());
            case Variant.Type.Object: return DynValue.FromObject(value.AsGodotObject());
            case Variant.Type.Array:
                var array = new List<DynValue>();
                foreach (var item in value.AsGodotArray()) array.Add(FromVariant(item, depth + 1));
                return DynValue.FromArray(array);
            case Variant.Type.Dictionary:
                var dict = new Dictionary<DynValue, DynValue>();
                foreach (var pair in value.AsGodotDictionary())
                    dict.Add(FromVariant(pair.Key, depth + 1), FromVariant(pair.Value, depth + 1));
                return DynValue.FromDictionary(dict);
            default: throw new NotSupportedException($"Godot Variant {value.VariantType} is not supported by the VM.");
        }
    }
}
