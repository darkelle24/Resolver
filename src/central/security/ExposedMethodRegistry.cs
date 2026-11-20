using System.Reflection;

namespace VMRuntime;

/// <summary>Thread-safe explicit host-method registry, independent of Godot.</summary>
public static class ExposedMethodRegistry
{
    private static readonly object Gate = new();
    private static readonly Dictionary<Type, HashSet<string>> Allowed = new();
    private static readonly Dictionary<Type, HashSet<string>> Excluded = new();
    private static readonly HashSet<Type> AllowAll = new();
    /// <summary>Registers public instance methods explicitly decorated with ExposedToVM.</summary>
    public static void InitializeFromAttributes(Assembly assembly)
    {
        ArgumentNullException.ThrowIfNull(assembly);
        foreach (var type in assembly.GetTypes())
            foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Instance))
                if (method.GetCustomAttribute<ExposedToVM>() is not null) RegisterMethod(type, method.Name);
    }
    /// <summary>Permits a named method on a type and its descendants.</summary>
    public static void RegisterMethod(Type targetType, string methodName)
    {
        ArgumentNullException.ThrowIfNull(targetType);
        ArgumentException.ThrowIfNullOrWhiteSpace(methodName);
        lock (Gate)
        {
            if (!Allowed.TryGetValue(targetType, out var methods)) Allowed[targetType] = methods = new(StringComparer.Ordinal);
            methods.Add(methodName);
        }
    }
    /// <summary>Excludes a method; exclusions anywhere in the type hierarchy take precedence.</summary>
    public static void RegisterExcludedMethod(Type targetType, string methodName)
    {
        ArgumentNullException.ThrowIfNull(targetType);
        ArgumentException.ThrowIfNullOrWhiteSpace(methodName);
        lock (Gate)
        {
            if (!Excluded.TryGetValue(targetType, out var methods)) Excluded[targetType] = methods = new(StringComparer.Ordinal);
            methods.Add(methodName);
        }
    }
    /// <summary>Enables or disables public instance methods for a type, subject to exclusions.</summary>
    public static void SetAllowAllMethods(Type targetType, bool allow)
    {
        ArgumentNullException.ThrowIfNull(targetType);
        lock (Gate) { if (allow) AllowAll.Add(targetType); else AllowAll.Remove(targetType); }
    }
    /// <summary>Checks explicit permissions across the complete inheritance hierarchy.</summary>
    public static bool IsMethodAllowed(object target, string methodName)
    {
        if (target is null) return false;
        lock (Gate)
        {
            bool allowed = false;
            for (Type? type = target.GetType(); type is not null && type != typeof(object); type = type.BaseType)
            {
                if (Excluded.TryGetValue(type, out var excluded) && excluded.Contains(methodName)) return false;
                allowed |= AllowAll.Contains(type) || (Allowed.TryGetValue(type, out var methods) && methods.Contains(methodName));
            }
            return allowed;
        }
    }
    /// <summary>Returns a detached list of permitted method names.</summary>
    public static IEnumerable<string> GetAllowedMethods(object target)
    {
        if (target is null) return Array.Empty<string>();
        lock (Gate)
        {
            var names = new HashSet<string>(StringComparer.Ordinal);
            for (Type? type = target.GetType(); type is not null && type != typeof(object); type = type.BaseType)
            {
                if (Allowed.TryGetValue(type, out var methods)) names.UnionWith(methods);
                if (AllowAll.Contains(type))
                    names.UnionWith(type.GetMethods(BindingFlags.Public | BindingFlags.Instance).Select(method => method.Name));
            }
            return names.Where(name => IsMethodAllowed(target, name)).OrderBy(name => name, StringComparer.Ordinal).ToArray();
        }
    }
}
