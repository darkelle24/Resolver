using VMRuntime.Diagnostics;

namespace VMRuntime;

/// <summary>JSON syntax conventions shared by every registered node.</summary>
public sealed class VmNodeSyntax : VmJsonObject
{
    public IReadOnlyList<string> OperationFields { get; } = Array.AsReadOnly(new[] { "operation", "type" });
    public string InputContainer => "inputs";
    public bool AllowDirectInputs => true;
    public string CanonicalParameterPlacement => "direct";
    public bool AllowParametersInInputContainer => true;
    public bool AllowUnknownFields => true;
    public bool LiteralArraysContainExpressions => false;
    public bool OperationNamesCaseSensitive => false;
    public bool FieldNamesCaseSensitive => true;
    public string PathFormat => "RFC 6901";
    public string InputOrder => "Definition order; variadic expressions preserve array order.";
    public IReadOnlyList<string> Shorthands { get; } = Array.AsReadOnly(new[]
    { "JSON primitive/array => constant", "{\"variable\":\"name\"} => read_variable", "{\"$type\":\"tag\",...} => constant" });
}

/// <summary>A typed literal available without an explicit operation field.</summary>
public sealed class VmLiteralTypeDefinition : VmJsonObject
{
    public string Tag { get; }
    public string Description { get; }
    public IReadOnlyList<VmNodeParameterDefinition> Components { get; }
    internal VmLiteralTypeDefinition(string tag, string description, params VmNodeParameterDefinition[] components)
        => (Tag, Description, Components) = (tag, description, Array.AsReadOnly(components.Select(value => value.Snapshot()).ToArray()));
}

/// <summary>Versioned, serializable list of the node definitions actually registered in this engine.</summary>
public sealed class VmNodeCatalog : VmJsonObject
{
    public int FormatVersion => 1;
    public long RegistryVersion { get; }
    public string RegistryHash { get; }
    public VmNodeSyntax Syntax { get; } = new();
    public IReadOnlyList<string> ValueTypes => VmNodeTypes.All;
    public IReadOnlyList<VmLiteralTypeDefinition> LiteralTypes { get; }
    public IReadOnlyList<VmNodeDefinition> Nodes { get; }
    internal VmNodeCatalog(long version, string hash, IEnumerable<VmNodeDefinition> nodes)
    {
        RegistryVersion = version; RegistryHash = hash;
        Nodes = Array.AsReadOnly(nodes.OrderBy(node => node.Operation, StringComparer.Ordinal).ToArray());
        LiteralTypes = VmBuiltinNodes.LiteralTypes;
    }
}
