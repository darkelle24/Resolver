// ValueType.cs
namespace VMRuntime;

/// <summary>All types supported by the VM. Enum values double as table indices.</summary>
public enum ValueType : byte
{
    Null = 0,
    Int,
    Float,
    Bool,
    Vector2,
    Vector2i,
    Vector3,
    Vector3i,
    Color,
    String,
    Array,
    Dictionary,
    Object   // opaque host references
}