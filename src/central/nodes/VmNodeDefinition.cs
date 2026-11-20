using System.Text.Json;
using VMRuntime.Diagnostics;

namespace VMRuntime;

/// <summary>Named child expression or ordered array of child expressions.</summary>
public sealed class VmNodeInputDefinition : VmJsonObject
{
    public string Name { get; init; } = "";
    public string Description { get; init; } = "";
    public IReadOnlyList<string> AcceptedTypes { get; init; } = ["any"];
    public bool Required { get; init; } = true;
    public bool Variadic { get; init; }
    public int MinItems { get; init; }
    public bool NullAsEmpty { get; init; }
    public JsonElement? DefaultValue { get; init; }
    public bool HasDefault => DefaultValue.HasValue;
    internal VmNodeInputDefinition Snapshot()
    {
        VmNodeDefinition.ValidateField(Name, Description);
        if (MinItems < 0 || (!Variadic && (MinItems != 0 || NullAsEmpty))) throw new ArgumentException("Invalid variadic input configuration.");
        if (Variadic && DefaultValue is { } defaultValue && defaultValue.ValueKind != JsonValueKind.Array)
            throw new ArgumentException("A variadic input default must be a JSON array.");
        return new()
        {
            Name = Name, Description = Description, AcceptedTypes = VmNodeDefinition.Types(AcceptedTypes),
            Required = Required, Variadic = Variadic, MinItems = MinItems, NullAsEmpty = NullAsEmpty,
            DefaultValue = DefaultValue?.Clone()
        };
    }
}

/// <summary>Compile-time literal parameter, kept separate from child expressions.</summary>
public sealed class VmNodeParameterDefinition : VmJsonObject
{
    public string Name { get; init; } = "";
    public string Description { get; init; } = "";
    public IReadOnlyList<string> AcceptedTypes { get; init; } = ["any"];
    public bool Required { get; init; } = true;
    public int MinLength { get; init; }
    public JsonElement? DefaultValue { get; init; }
    public bool HasDefault => DefaultValue.HasValue;
    public IReadOnlyList<string> EnumValues { get; init; } = [];
    internal VmNodeParameterDefinition Snapshot()
    {
        VmNodeDefinition.ValidateField(Name, Description);
        var types = VmNodeDefinition.Types(AcceptedTypes);
        if (MinLength < 0) throw new ArgumentOutOfRangeException(nameof(MinLength));
        ArgumentNullException.ThrowIfNull(EnumValues);
        if (EnumValues.Any(value => value is null)) throw new ArgumentException("Enum values cannot be null.");
        if (EnumValues.Count > 0 && !types.Contains("string") && !types.Contains("any"))
            throw new ArgumentException("String enum values require an accepted string type.");
        return new()
        {
            Name = Name, Description = Description, AcceptedTypes = types, Required = Required,
            MinLength = MinLength, DefaultValue = DefaultValue?.Clone(), EnumValues = Array.AsReadOnly(EnumValues.Distinct(StringComparer.Ordinal).ToArray())
        };
    }
}

/// <summary>Possible output types and the rule used to determine the actual output.</summary>
public sealed class VmNodeOutputDefinition : VmJsonObject
{
    public IReadOnlyList<string> Types { get; init; } = ["any"];
    public string Description { get; init; } = "Result of this expression.";
    public string Inference { get; init; } = "runtime";
    internal VmNodeOutputDefinition Snapshot()
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(Description);
        ArgumentException.ThrowIfNullOrWhiteSpace(Inference);
        return new() { Types = VmNodeDefinition.Types(Types), Description = Description, Inference = Inference };
    }
}

/// <summary>Single source of truth for node syntax, editor documentation and compilation contracts.</summary>
public sealed class VmNodeDefinition : VmJsonObject
{
    public string Operation { get; init; } = "";
    public string Title { get; init; } = "";
    public string Category { get; init; } = "project";
    public string Description { get; init; } = "";
    public int Version { get; init; } = 1;
    public IReadOnlyList<string> Aliases { get; init; } = [];
    public IReadOnlyList<VmNodeInputDefinition> Inputs { get; init; } = [];
    public IReadOnlyList<VmNodeParameterDefinition> Parameters { get; init; } = [];
    public VmNodeOutputDefinition Output { get; init; } = new();
    public IReadOnlyList<JsonElement> Examples { get; init; } = [];
    public string Evaluation { get; init; } = "eager";
    public bool Deprecated { get; init; }
    public string Origin { get; init; } = "project";

    internal static void ValidateField(string name, string description)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(description);
        if (name is "operation" or "type" or "inputs" or "$type" || name.IndexOfAny(['/', '~']) >= 0)
            throw new ArgumentException($"Reserved or invalid node field name '{name}'.");
    }
    internal static string OperationId(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        string id = name.ToLowerInvariant();
        if (!char.IsAsciiLetterLower(id[0]) || id.Any(c => !char.IsAsciiLetterLower(c) && !char.IsAsciiDigit(c) && c is not ('_' or '.' or ':' or '-')))
            throw new ArgumentException($"Invalid node operation identifier '{name}'.");
        return id;
    }
    internal static IReadOnlyList<string> Types(IReadOnlyList<string> values)
    {
        ArgumentNullException.ThrowIfNull(values);
        var result = values.Select(value => value?.ToLowerInvariant() ?? "").Distinct(StringComparer.Ordinal).ToArray();
        if (result.Length == 0 || result.Any(value => value != "any" && !VmNodeTypes.All.Contains(value)))
            throw new ArgumentException("Accepted types must name VM types or 'any'.");
        if (result.Contains("any") && result.Length != 1) throw new ArgumentException("'any' cannot be combined with specific types.");
        return Array.AsReadOnly(result);
    }
    internal VmNodeDefinition Snapshot(string? evaluation = null, string? origin = null)
    {
        string operation = OperationId(Operation);
        ArgumentException.ThrowIfNullOrWhiteSpace(Description);
        ArgumentException.ThrowIfNullOrWhiteSpace(Category);
        if (Version <= 0) throw new ArgumentOutOfRangeException(nameof(Version));
        ArgumentNullException.ThrowIfNull(Aliases);
        ArgumentNullException.ThrowIfNull(Inputs);
        ArgumentNullException.ThrowIfNull(Parameters);
        ArgumentNullException.ThrowIfNull(Examples);
        ArgumentNullException.ThrowIfNull(Output);
        var aliases = Aliases.Select(OperationId).Distinct(StringComparer.Ordinal).ToArray();
        if (aliases.Contains(operation)) throw new ArgumentException("An alias cannot equal the canonical operation.");
        var inputs = Inputs.Select(value => (value ?? throw new ArgumentException("Null input definition.")).Snapshot()).ToArray();
        var parameters = Parameters.Select(value => (value ?? throw new ArgumentException("Null parameter definition.")).Snapshot()).ToArray();
        var names = inputs.Select(value => value.Name).Concat(parameters.Select(value => value.Name)).ToArray();
        if (names.Distinct(StringComparer.Ordinal).Count() != names.Length) throw new ArgumentException("Input and parameter names must be unique.");
        string strategy = evaluation ?? Evaluation;
        if (strategy is not ("eager" or "literal" or "variable" or "conditional" or "short_circuit" or "loop" or "sequence" or "custom"))
            throw new ArgumentException($"Unknown evaluation strategy '{strategy}'.");
        return new()
        {
            Operation = operation, Title = string.IsNullOrWhiteSpace(Title) ? operation : Title,
            Category = Category, Description = Description, Version = Version,
            Aliases = Array.AsReadOnly(aliases), Inputs = Array.AsReadOnly(inputs), Parameters = Array.AsReadOnly(parameters),
            Output = Output.Snapshot(), Examples = Array.AsReadOnly(Examples.Select(value => value.Clone()).ToArray()),
            Evaluation = strategy, Deprecated = Deprecated, Origin = origin ?? Origin
        };
    }
}

internal static class VmNodeTypes
{
    internal static readonly IReadOnlyList<string> All = Array.AsReadOnly(Enum.GetValues<ValueType>().Select(v => v.ToString().ToLowerInvariant()).ToArray());
    internal static bool Accepts(IReadOnlyList<string> types, DynValue value)
        => types.Contains("any") || types.Contains(value.Type.ToString().ToLowerInvariant());
}
