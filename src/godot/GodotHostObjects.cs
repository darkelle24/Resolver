#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
namespace VMRuntime;

/// <summary>Godot object adapter. Method calls use the existing explicit exposure registry.</summary>
public sealed class GodotHostObjects : IVmHostObjects
{
    private static GodotObject Target(object value)
    {
        if (value is not GodotObject target || !GodotObject.IsInstanceValid(target))
            throw new InvalidOperationException("The Godot target is invalid or has been freed.");
        return target;
    }
    /// <summary>Reads an existing Godot property.</summary>
    public DynValue Get(object target, string property)
    {
        var obj = Target(target);
        if (!GetProperties(obj).Contains(property, StringComparer.Ordinal)) throw new MissingMemberException(property);
        return DynValueConverter.FromVariant(obj.Get(property));
    }
    /// <summary>Writes an existing Godot property.</summary>
    public void Set(object target, string property, DynValue value)
    {
        var obj = Target(target);
        if (!GetProperties(obj).Contains(property, StringComparer.Ordinal)) throw new MissingMemberException(property);
        obj.Set(property, DynValueConverter.ToVariant(value));
    }
    /// <summary>Calls only methods explicitly permitted by ExposedMethodRegistry.</summary>
    public DynValue Call(object target, string method, IReadOnlyList<DynValue> arguments)
    {
        var obj = Target(target);
        if (!ExposedMethodRegistry.IsMethodAllowed(obj, method)) throw new UnauthorizedAccessException($"Method '{method}' is not exposed to scripts.");
        if (!obj.HasMethod(method)) throw new MissingMethodException(method);
        return DynValueConverter.FromVariant(obj.Call(method, arguments.Select(DynValueConverter.ToVariant).ToArray()));
    }
    /// <summary>Checks validity without invoking the object.</summary>
    public bool IsValid(object target) => target is GodotObject obj && GodotObject.IsInstanceValid(obj);
    /// <summary>Frees a valid Godot object.</summary>
    public void Free(object target) => Target(target).Free();
    /// <summary>Lists names from Godot's property metadata.</summary>
    public IReadOnlyList<string> GetProperties(object target)
        => Target(target).GetPropertyList().Select(p => p["name"].AsString()).ToArray();
    /// <summary>Lists exposed methods that exist on the object.</summary>
    public IReadOnlyList<string> GetMethods(object target)
    {
        var obj = Target(target);
        return ExposedMethodRegistry.GetAllowedMethods(obj).Where(name => obj.HasMethod(name)).ToArray();
    }
}
