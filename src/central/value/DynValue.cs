using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace VMRuntime;

[StructLayout(LayoutKind.Explicit, Size = 16)]
internal struct ValueUnion
{
    [FieldOffset(0)] public int IntValue;
    [FieldOffset(0)] public float FloatValue;
    [FieldOffset(0)] public bool BoolValue;
    [FieldOffset(0)] public ScriptVector2 Vector2Value;
    [FieldOffset(0)] public ScriptVector2I Vector2iValue;
    [FieldOffset(0)] public ScriptVector3 Vector3Value;
    [FieldOffset(0)] public ScriptVector3I Vector3iValue;
    [FieldOffset(0)] public ScriptColor ColorValue;
}

/// <summary>Engine-independent tagged value. Collections and host objects retain reference identity.</summary>
public readonly struct DynValue : IEquatable<DynValue>
{
    /// <summary>Gets the immutable script type.</summary>
    public ValueType Type { get; }
    internal readonly ValueUnion Data;
    /// <summary>Gets the managed payload; prefer the checked accessors.</summary>
    public object? RefValue { get; }
    /// <summary>Represents the absence of a value; also the default struct value.</summary>
    public static readonly DynValue Null = default;
    /// <summary>Whether this value is null.</summary>
    public bool IsNull => Type == ValueType.Null;
    /// <summary>Whether this value is an array.</summary>
    public bool IsArray => Type == ValueType.Array;
    /// <summary>Whether this value is a dictionary.</summary>
    public bool IsDict => Type == ValueType.Dictionary;

    private DynValue(ValueType type, ValueUnion data = default, object? reference = null)
        => (Type, Data, RefValue) = (type, data, reference);

    /// <summary>Creates an integer without boxing.</summary>
    public static DynValue FromInt(int value) => new(ValueType.Int, new() { IntValue = value });
    /// <summary>Creates a floating-point value without boxing.</summary>
    public static DynValue FromFloat(float value) => new(ValueType.Float, new() { FloatValue = value });
    /// <summary>Creates a boolean without boxing.</summary>
    public static DynValue FromBool(bool value) => new(ValueType.Bool, new() { BoolValue = value });
    /// <summary>Creates a two-dimensional floating-point vector.</summary>
    public static DynValue FromVector2(ScriptVector2 value) => new(ValueType.Vector2, new() { Vector2Value = value });
    /// <summary>Creates a two-dimensional integer vector.</summary>
    public static DynValue FromVector2i(ScriptVector2I value) => new(ValueType.Vector2i, new() { Vector2iValue = value });
    /// <summary>Creates a three-dimensional floating-point vector.</summary>
    public static DynValue FromVector3(ScriptVector3 value) => new(ValueType.Vector3, new() { Vector3Value = value });
    /// <summary>Creates a three-dimensional integer vector.</summary>
    public static DynValue FromVector3i(ScriptVector3I value) => new(ValueType.Vector3i, new() { Vector3iValue = value });
    /// <summary>Creates an RGBA color.</summary>
    public static DynValue FromColor(ScriptColor value) => new(ValueType.Color, new() { ColorValue = value });
    /// <summary>Creates a non-null string; null is rejected.</summary>
    public static DynValue FromString(string value)
        => new(ValueType.String, reference: value ?? throw new ArgumentNullException(nameof(value)));
    /// <summary>Stores an opaque host reference; null becomes DynValue.Null.</summary>
    public static DynValue FromObject(object? value)
        => value is null ? Null : new(ValueType.Object, reference: value);
    /// <summary>Stores the supplied list without copying it.</summary>
    public static DynValue FromArray(List<DynValue> value)
        => new(ValueType.Array, reference: value ?? throw new ArgumentNullException(nameof(value)));
    /// <summary>Stores the supplied dictionary without copying it.</summary>
    public static DynValue FromDictionary(Dictionary<DynValue, DynValue> value)
        => new(ValueType.Dictionary, reference: value ?? throw new ArgumentNullException(nameof(value)));

    /// <summary>Reads an integer. Throws VmValueCastException for a different tag.</summary>
    public int AsInt() { Require(ValueType.Int); return Data.IntValue; }
    /// <summary>Reads a float without numeric coercion. Throws VmValueCastException on mismatch.</summary>
    public float AsFloat() { Require(ValueType.Float); return Data.FloatValue; }
    /// <summary>Reads a boolean. Throws VmValueCastException on mismatch.</summary>
    public bool AsBool() { Require(ValueType.Bool); return Data.BoolValue; }
    /// <summary>Reads a Vector2 shim. Throws VmValueCastException on mismatch.</summary>
    public ScriptVector2 AsVector2() { Require(ValueType.Vector2); return Data.Vector2Value; }
    /// <summary>Reads a Vector2I shim. Throws VmValueCastException on mismatch.</summary>
    public ScriptVector2I AsVector2i() { Require(ValueType.Vector2i); return Data.Vector2iValue; }
    /// <summary>Reads a Vector3 shim. Throws VmValueCastException on mismatch.</summary>
    public ScriptVector3 AsVector3() { Require(ValueType.Vector3); return Data.Vector3Value; }
    /// <summary>Reads a Vector3I shim. Throws VmValueCastException on mismatch.</summary>
    public ScriptVector3I AsVector3i() { Require(ValueType.Vector3i); return Data.Vector3iValue; }
    /// <summary>Reads a color shim. Throws VmValueCastException on mismatch.</summary>
    public ScriptColor AsColor() { Require(ValueType.Color); return Data.ColorValue; }
    /// <summary>Reads a string. Throws VmValueCastException on mismatch.</summary>
    public string AsString() { Require(ValueType.String); return (string)RefValue!; }
    /// <summary>Reads an opaque non-null host reference. Throws VmValueCastException on mismatch.</summary>
    public object AsObject() { Require(ValueType.Object); return RefValue!; }
    /// <summary>Reads a host reference compatible with T, otherwise throws a detailed cast exception.</summary>
    public T AsObject<T>() where T : class
        => Type == ValueType.Object && RefValue is T value ? value
            : throw new VmValueCastException($"Object<{typeof(T).FullName}>", this, nameof(AsObject));
    /// <summary>Reads the mutable array reference. Throws VmValueCastException on mismatch.</summary>
    public List<DynValue> AsArray() { Require(ValueType.Array); return (List<DynValue>)RefValue!; }
    /// <summary>Reads the mutable dictionary reference. Throws VmValueCastException on mismatch.</summary>
    public Dictionary<DynValue, DynValue> AsDictionary()
    { Require(ValueType.Dictionary); return (Dictionary<DynValue, DynValue>)RefValue!; }

    private void Require(ValueType expected, [CallerMemberName] string accessor = "")
    {
        if (Type != expected) throw new VmValueCastException(expected.ToString(), this, accessor);
    }

    /// <summary>Compares scalar values exactly and collections/objects by reference identity.</summary>
    public bool Equals(DynValue other)
    {
        if (Type != other.Type) return false;
        return Type switch
        {
            ValueType.Null => true,
            ValueType.Int => Data.IntValue == other.Data.IntValue,
            ValueType.Float => Data.FloatValue.Equals(other.Data.FloatValue),
            ValueType.Bool => Data.BoolValue == other.Data.BoolValue,
            ValueType.Vector2 => Data.Vector2Value.Equals(other.Data.Vector2Value),
            ValueType.Vector2i => Data.Vector2iValue.Equals(other.Data.Vector2iValue),
            ValueType.Vector3 => Data.Vector3Value.Equals(other.Data.Vector3Value),
            ValueType.Vector3i => Data.Vector3iValue.Equals(other.Data.Vector3iValue),
            ValueType.Color => Data.ColorValue.Equals(other.Data.ColorValue),
            ValueType.String => string.Equals((string?)RefValue, (string?)other.RefValue, StringComparison.Ordinal),
            _ => ReferenceEquals(RefValue, other.RefValue)
        };
    }

    /// <summary>Compares another CLR object with this script value.</summary>
    public override bool Equals(object? obj) => obj is DynValue value && Equals(value);
    /// <summary>Computes a hash consistent with exact scalar and reference equality.</summary>
    public override int GetHashCode() => HashCode.Combine(Type, Type switch
    {
        ValueType.Null => 0,
        ValueType.Int => Data.IntValue.GetHashCode(),
        ValueType.Float => Data.FloatValue.GetHashCode(),
        ValueType.Bool => Data.BoolValue.GetHashCode(),
        ValueType.Vector2 => Data.Vector2Value.GetHashCode(),
        ValueType.Vector2i => Data.Vector2iValue.GetHashCode(),
        ValueType.Vector3 => Data.Vector3Value.GetHashCode(),
        ValueType.Vector3i => Data.Vector3iValue.GetHashCode(),
        ValueType.Color => Data.ColorValue.GetHashCode(),
        ValueType.String => StringComparer.Ordinal.GetHashCode((string)RefValue!),
        _ => RefValue is null ? 0 : RuntimeHelpers.GetHashCode(RefValue)
    });
    /// <summary>Tests equality.</summary>
    public static bool operator ==(DynValue left, DynValue right) => left.Equals(right);
    /// <summary>Tests inequality.</summary>
    public static bool operator !=(DynValue left, DynValue right) => !left.Equals(right);

    /// <summary>Produces a bounded, non-recursive diagnostic preview, never invoking host ToString.</summary>
    public override string ToString() => Type switch
    {
        ValueType.Null => "null",
        ValueType.Int => Data.IntValue.ToString(CultureInfo.InvariantCulture),
        ValueType.Float => Data.FloatValue.ToString("R", CultureInfo.InvariantCulture),
        ValueType.Bool => Data.BoolValue ? "true" : "false",
        ValueType.String => System.Text.Json.JsonSerializer.Serialize(
            ((string)RefValue!)[..Math.Min(120, ((string)RefValue!).Length)]),
        ValueType.Vector2 => Data.Vector2Value.ToString(),
        ValueType.Vector2i => Data.Vector2iValue.ToString(),
        ValueType.Vector3 => Data.Vector3Value.ToString(),
        ValueType.Vector3i => Data.Vector3iValue.ToString(),
        ValueType.Color => Data.ColorValue.ToString(),
        ValueType.Array => $"Array(count={AsArray().Count})",
        ValueType.Dictionary => $"Dictionary(count={AsDictionary().Count})",
        _ => $"Object<{RefValue?.GetType().FullName}>"
    };
}

/// <summary>Checked-access failure carrying the expected type and actual value.</summary>
public sealed class VmValueCastException : InvalidCastException
{
    /// <summary>Expected VM or CLR type.</summary>
    public string ExpectedType { get; }
    /// <summary>Actual VM type.</summary>
    public string ActualType { get; }
    /// <summary>Creates a detailed cast diagnostic without converting the value.</summary>
    public VmValueCastException(string expectedType, DynValue actual, string accessor)
        : base($"{accessor}: expected {expectedType}, got {actual.Type}; CLR payload " +
               $"{actual.RefValue?.GetType().FullName ?? actual.Type.ToString()}; value={actual}.")
        => (ExpectedType, ActualType) = (expectedType, actual.Type.ToString());
}
