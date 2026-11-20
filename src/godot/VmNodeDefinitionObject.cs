#nullable enable
using System;
using System.Linq;
using System.Text.Json;
using Godot;
namespace VMRuntime.GodotApi;

/// <summary>Typed syntax and documentation supplied when registering a GDScript/C# callback node.</summary>
[GlobalClass]
public partial class VmNodeDefinitionObject : RefCounted
{
    public string Operation { get; set; } = "";
    public string Title { get; set; } = "";
    public string Category { get; set; } = "project";
    public string Description { get; set; } = "";
    public int Version { get; set; } = 1;
    public Godot.Collections.Array<string> Aliases { get; set; } = new();
    public Godot.Collections.Array<VmNodeInputDefinitionObject> Inputs { get; set; } = new();
    public Godot.Collections.Array<VmNodeParameterDefinitionObject> Parameters { get; set; } = new();
    public VmNodeOutputDefinitionObject Output { get; set; } = new();
    public string ExamplesJson { get; set; } = "[]";
    public bool Deprecated { get; set; }
    private string _evaluation = "eager";
    private string _origin = "project";
    public string Evaluation => _evaluation;
    public string Origin => _origin;
    /// <summary>Prints this node definition as a standard catalog entry.</summary>
    public string ToJson() => ToJson(true);
    /// <summary>Prints with explicit indentation.</summary>
    public string ToJson(bool indented) => ToCore().ToJson(indented);
    internal VmNodeDefinition ToCore()
    {
        ArgumentNullException.ThrowIfNull(Inputs);
        ArgumentNullException.ThrowIfNull(Parameters);
        ArgumentNullException.ThrowIfNull(Output);
        using var examples = JsonDocument.Parse(ExamplesJson);
        if (examples.RootElement.ValueKind != JsonValueKind.Array) throw new ArgumentException("ExamplesJson must contain an array.");
        return new()
        {
            Operation = Operation, Title = Title, Category = Category, Description = Description, Version = Version,
            Aliases = NodeDefinitionJson.Strings(Aliases),
            Inputs = Inputs.Select(input => (input ?? throw new ArgumentException("Null input definition.")).ToCore()).ToArray(),
            Parameters = Parameters.Select(parameter => (parameter ?? throw new ArgumentException("Null parameter definition.")).ToCore()).ToArray(),
            Output = Output.ToCore(), Examples = examples.RootElement.EnumerateArray().Select(value => value.Clone()).ToArray(),
            Evaluation = _evaluation, Origin = _origin, Deprecated = Deprecated
        };
    }
    internal static VmNodeDefinitionObject Wrap(VmNodeDefinition data)
    {
        var result = new VmNodeDefinitionObject
        {
            Operation = data.Operation, Title = data.Title, Category = data.Category, Description = data.Description,
            Version = data.Version, Aliases = new(data.Aliases), Output = VmNodeOutputDefinitionObject.Wrap(data.Output),
            ExamplesJson = JsonSerializer.Serialize(data.Examples), Deprecated = data.Deprecated,
            _evaluation = data.Evaluation, _origin = data.Origin
        };
        foreach (var input in data.Inputs) result.Inputs.Add(VmNodeInputDefinitionObject.Wrap(input));
        foreach (var parameter in data.Parameters) result.Parameters.Add(VmNodeParameterDefinitionObject.Wrap(parameter));
        return result;
    }
}
internal static class NodeDefinitionJson
{
    internal static string[] Strings(Godot.Collections.Array<string>? values)
        => (values ?? throw new ArgumentNullException(nameof(values))).ToArray();
    internal static JsonElement? Default(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return null;
        using var json = JsonDocument.Parse(text);
        return json.RootElement.Clone();
    }
}
