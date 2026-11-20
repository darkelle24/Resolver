using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using VMRuntime.Diagnostics;

namespace VMRuntime;

/// <summary>Compiles JSON expressions using the same registered definitions exported to editor catalogs.</summary>
public sealed class VmJsonCompiler
{
    private readonly VmNodeRegistry _registry;
    /// <summary>Creates a compiler with built-ins, or with a supplied project registry.</summary>
    public VmJsonCompiler(VmNodeRegistry? registry = null) => _registry = registry ?? new();

    /// <summary>Compiles JSON text with positions computed from that original text.</summary>
    public VmCompilationResult Compile(string json, string sourceFile = "<json>", string? scriptId = null)
        => Compile(json, sourceFile, scriptId, _registry.Snapshot());
    internal VmCompilationResult Compile(string json, string sourceFile, string? scriptId, VmNodeRegistrySnapshot nodes)
    {
        ArgumentNullException.ThrowIfNull(json);
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceFile);
        try
        {
            var source = new LocatedJson(json, sourceFile);
            var state = new Compilation(source, nodes);
            state.Expression(source.Root, "", null);
            return new(new CompiledScript(scriptId ?? sourceFile, sourceFile, Hash(json), json, state.Program,
                nodeCatalog: nodes.Catalog), null);
        }
        catch (VmCompileFault fault) { return new(null, fault.Error); }
    }

    /// <summary>Compiles only a selected parsed fragment, retaining merger-provided metadata.</summary>
    public VmCompilationResult CompileFragment(JsonNode? fragment, VmCompilationMetadata metadata)
    {
        ArgumentNullException.ThrowIfNull(metadata);
        try { return CompileFragmentJson(VmFragmentJson.Serialize(fragment), metadata); }
        catch (Exception error) when (error is ArgumentException or InvalidOperationException or JsonException)
        { return InvalidFragment(error, metadata); }
    }

    /// <summary>Compiles supplied fragment text without opening a source file.</summary>
    public VmCompilationResult CompileFragmentJson(string json, VmCompilationMetadata metadata)
        => CompileFragmentJson(json, metadata, _registry.Snapshot());
    internal VmCompilationResult CompileFragmentJson(string json, VmCompilationMetadata metadata, VmNodeRegistrySnapshot nodes)
    {
        ArgumentNullException.ThrowIfNull(json);
        ArgumentNullException.ThrowIfNull(metadata);
        try
        {
            var context = metadata.Snapshot();
            var source = new LocatedJson(json, context.SourceFile, context);
            var state = new Compilation(source, nodes);
            state.Expression(source.Root, "", null);
            return new(new CompiledScript(context.EffectiveId, context.SourceFile, Hash(json), json, state.Program, context, true,
                nodes.Catalog), null);
        }
        catch (VmCompileFault fault) { return new(null, fault.Error); }
        catch (ArgumentException error) { return InvalidFragment(error, metadata, "INVALID_METADATA"); }
    }
    private static string Hash(string json)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(json))).ToLowerInvariant();
    internal static VmCompilationResult InvalidFragment(Exception error, VmCompilationMetadata metadata, string code = "JSON_INPUT_TYPE")
        => new(null, new VmCompilationError(code, error.Message,
            new(string.IsNullOrWhiteSpace(metadata.SourceFile) ? "<fragment>" : metadata.SourceFile,
                metadata.RootPath is { Length: > 0 } path && path[0] == '/' ? path : "", 0, 0)));

    private sealed record ChildExpression(string Key, string Path, JsonNode? Json, VmNodeInputDefinition Definition);
    private sealed class Compilation(LocatedJson source, VmNodeRegistrySnapshot nodes)
    {
        internal Program Program { get; } = new();
        private int Emit(OpcodeId opcode, int node, int a = 0, int b = 0, int c = 0)
            => Program.Emit(new() { Opcode = opcode, a = a, b = b, c = c }, node);
        private int Constant(DynValue value) => Program.AddConstant(value);
        private VmCompileFault Error(string code, string message, string path)
            => new(new VmCompilationError(code, message, source.Locate(path)));
        private (JsonNode? Value, string Path, bool Present) Field(JsonObject obj, string name, string path, bool required = true)
        {
            if (obj.ContainsKey("inputs") && obj["inputs"] is not JsonObject)
                throw Error("JSON_TYPE", "'inputs' must be an object.", LocatedJson.Child(path, "inputs"));
            bool nested = obj["inputs"] is JsonObject inputs && inputs.ContainsKey(name);
            bool direct = obj.ContainsKey(name);
            if (nested && direct) throw Error("JSON_AMBIGUOUS_INPUT", $"Input '{name}' is defined twice.", path);
            if (nested) return (((JsonObject)obj["inputs"]!)[name], LocatedJson.Child(LocatedJson.Child(path, "inputs"), name), true);
            if (direct) return (obj[name], LocatedJson.Child(path, name), true);
            if (required) throw new VmCompileFault(new VmMissingFieldError(name, source.Locate(path)));
            return (null, LocatedJson.Child(path, name), false);
        }
        private string Text(JsonNode? node, string path)
        {
            if (node is JsonValue value && value.TryGetValue<string>(out var text) && !string.IsNullOrWhiteSpace(text)) return text;
            throw Error("JSON_TYPE", "Expected a non-empty string.", path);
        }
        private static JsonNode? Default(JsonElement? value) => value.HasValue ? JsonNode.Parse(value.Value.GetRawText()) : null;

        internal void Expression(JsonNode? json, string path, string? parentInput, int? parentNodeId = null)
        {
            if (Program.Nodes.Count >= 100_000) throw Error("JSON_NODE_LIMIT", "Script has too many operations.", path);
            var obj = json as JsonObject;
            bool shorthandConstant = obj is null || obj.ContainsKey("$type");
            string requested;
            if (shorthandConstant) requested = "constant";
            else if (obj!.ContainsKey("variable") && !obj.ContainsKey("operation") && !obj.ContainsKey("type")) requested = "read_variable";
            else
            {
                string tag = obj!.ContainsKey("operation") ? "operation" : "type";
                if (!obj.ContainsKey(tag)) throw new VmCompileFault(new VmMissingFieldError("operation", source.Locate(path)));
                requested = Text(obj[tag], LocatedJson.Child(path, tag));
            }
            var registration = nodes.Find(requested) ?? throw Error("JSON_UNKNOWN_OPERATION", $"Unknown operation '{requested}'.", path);
            var definition = registration.Definition;
            var parameters = new Dictionary<string, DynValue>(StringComparer.Ordinal);
            foreach (var parameter in definition.Parameters)
            {
                DynValue value;
                string parameterPath;
                bool present = true;
                if (registration.Kind == VmNodeKind.Constant && shorthandConstant)
                { value = Literal(json, path); parameterPath = path; }
                else
                {
                    string key = registration.Kind == VmNodeKind.Variable && obj!.ContainsKey("variable") ? "variable" : parameter.Name;
                    var field = Field(obj!, key, path, parameter.Required);
                    parameterPath = field.Path; present = field.Present;
                    value = Literal(field.Present ? field.Value : Default(parameter.DefaultValue), parameterPath);
                }
                if ((present || parameter.DefaultValue.HasValue) && !VmNodeTypes.Accepts(parameter.AcceptedTypes, value))
                    throw Error("JSON_TYPE", $"Parameter '{parameter.Name}' expects {string.Join('|', parameter.AcceptedTypes)}, got {value.Type}.", parameterPath);
                if (parameter.MinLength > 0 && value.Type == ValueType.String &&
                    (string.IsNullOrWhiteSpace(value.AsString()) || value.AsString().Length < parameter.MinLength))
                    throw Error("JSON_TYPE", $"Parameter '{parameter.Name}' must contain at least {parameter.MinLength} non-empty characters.", parameterPath);
                if ((present || parameter.DefaultValue.HasValue) && parameter.EnumValues.Count > 0 && (value.Type != ValueType.String || !parameter.EnumValues.Contains(value.AsString(), StringComparer.Ordinal)))
                    throw Error("JSON_ENUM", $"Parameter '{parameter.Name}' must be one of: {string.Join(", ", parameter.EnumValues)}.", parameterPath);
                parameters.Add(parameter.Name, value);
            }

            var children = new List<ChildExpression>();
            foreach (var input in definition.Inputs)
            {
                var field = Field(obj!, input.Name, path, input.Required);
                JsonNode? value = field.Present ? field.Value : input.DefaultValue.HasValue
                    ? Default(input.DefaultValue) : input.Variadic ? new JsonArray() : null;
                if (!input.Variadic) children.Add(new(input.Name, field.Path, value, input));
                else
                {
                    if (value is null && input.NullAsEmpty) value = new JsonArray();
                    if (value is not JsonArray array) throw Error("JSON_TYPE", $"'{input.Name}' must be an array of expressions.", field.Path);
                    if (array.Count < input.MinItems) throw Error("JSON_INPUT_COUNT", $"'{input.Name}' requires at least {input.MinItems} expressions.", field.Path);
                    for (int i = 0; i < array.Count; i++)
                        children.Add(new(input.Name + "/" + i, LocatedJson.Child(field.Path, i.ToString()), array[i], input));
                }
            }
            int node = Program.AddNode(new(Program.Nodes.Count, source.Locate(path), definition.Operation, parentInput,
                parameters.ToDictionary(p => p.Key, p => VmValueSnapshot.Capture(p.Value)), children.Select(c => c.Key),
                parentNodeId, source.LogicalPath(path), definition.Output.Types,
                Array.AsReadOnly(children.Select(child => child.Definition.AcceptedTypes).ToArray())));
            Emit(OpcodeId.EnterNode, node, node);
            void Child(string name)
            {
                for (int i = 0; i < children.Count; i++)
                    if (children[i].Definition.Name == name)
                    {
                        var child = children[i];
                        Expression(child.Json, child.Path, child.Key, node);
                        if (registration.Kind == VmNodeKind.CustomCompiler && !child.Definition.AcceptedTypes.Contains("any"))
                            Emit(OpcodeId.AssertNodeInput, node, node, i);
                    }
            }
            void All() { foreach (var child in children) Expression(child.Json, child.Path, child.Key, node); }
            switch (registration.Kind)
            {
                case VmNodeKind.Constant: Emit(OpcodeId.PushConstant, node, Constant(parameters["value"])); break;
                case VmNodeKind.Variable: Emit(OpcodeId.LoadVariable, node, Constant(parameters["name"])); break;
                case VmNodeKind.Opcode: All(); Emit(registration.Opcode!.Value, node); break;
                case VmNodeKind.If:
                {
                    Child("condition");
                    int branch = Emit(OpcodeId.JumpIfFalse, node, b: 1);
                    Child("then");
                    int end = Emit(OpcodeId.Jump, node);
                    Program.PatchTarget(branch, Program.Count);
                    Child("else");
                    Program.PatchTarget(end, Program.Count);
                    break;
                }
                case VmNodeKind.And: case VmNodeKind.Or:
                {
                    Child("left");
                    int branch = Emit(registration.Kind == VmNodeKind.And ? OpcodeId.JumpIfFalse : OpcodeId.JumpIfTrue, node, b: 1);
                    Child("right");
                    Emit(OpcodeId.AssertBool, node);
                    int end = Emit(OpcodeId.Jump, node);
                    Program.PatchTarget(branch, Program.Count);
                    Emit(OpcodeId.PushConstant, node, Constant(DynValue.FromBool(registration.Kind == VmNodeKind.Or)));
                    Program.PatchTarget(end, Program.Count);
                    break;
                }
                case VmNodeKind.While:
                {
                    Emit(OpcodeId.PushConstant, node, Constant(DynValue.Null));
                    int start = Program.Count;
                    Child("condition");
                    int end = Emit(OpcodeId.JumpIfFalse, node, b: 1);
                    Emit(OpcodeId.Pop, node);
                    Child("body");
                    Emit(OpcodeId.Jump, node, start);
                    Program.PatchTarget(end, Program.Count);
                    break;
                }
                case VmNodeKind.Array: All(); Emit(OpcodeId.MakeArray, node, children.Count, 1); break;
                case VmNodeKind.Sequence:
                    for (int i = 0; i < children.Count; i++)
                    {
                        var child = children[i];
                        Expression(child.Json, child.Path, child.Key, node);
                        if (i < children.Count - 1) Emit(OpcodeId.Pop, node);
                    }
                    if (children.Count == 0) Emit(OpcodeId.PushConstant, node, Constant(DynValue.Null));
                    break;
                case VmNodeKind.Call:
                    All();
                    Emit(OpcodeId.CallFunction, node, Constant(parameters["function"]), children.Count);
                    break;
                case VmNodeKind.ObjectCall:
                    Child("target");
                    Emit(OpcodeId.PushConstant, node, Constant(parameters["method"]));
                    Child("args");
                    Emit(OpcodeId.ObjectCall, node, children.Count - 1, 1);
                    break;
                case VmNodeKind.Callback:
                    All();
                    int binding = Program.AddNodeBinding(new(definition, registration.Callback!,
                        Array.AsReadOnly(children.Select(child => new VmBoundNodeInput(child.Key, child.Definition)).ToArray()),
                        new System.Collections.ObjectModel.ReadOnlyDictionary<string, DynValue>(parameters)));
                    Emit(OpcodeId.InvokeNode, node, binding, children.Count);
                    break;
                case VmNodeKind.CustomCompiler:
                {
                    var owned = new HashSet<int>();
                    int CustomEmit(OpcodeId opcode, int a, int b, int c)
                    {
                        if (opcode is OpcodeId.EnterNode or OpcodeId.ExitNode or OpcodeId.InvokeNode or OpcodeId.AssertNodeInput)
                            throw new ArgumentException("Node frame and binding instructions are compiler-owned.");
                        int index = Emit(opcode, node, a, b, c);
                        owned.Add(index); return index;
                    }
                    var context = new VmNodeCompilationContext(definition,
                        new System.Collections.ObjectModel.ReadOnlyDictionary<string, DynValue>(parameters),
                        name =>
                        {
                            if (!definition.Inputs.Any(input => input.Name == name)) throw new ArgumentException($"Unknown input '{name}'.");
                            Child(name);
                            return children.Count(child => child.Definition.Name == name);
                        }, CustomEmit, Constant, () => Program.Count,
                        (index, target) =>
                        {
                            if (!owned.Contains(index) || Program[index].Opcode is not (OpcodeId.Jump or OpcodeId.JumpIfFalse or OpcodeId.JumpIfTrue))
                                throw new ArgumentException("Only jumps emitted by this custom node can be patched.");
                            if (target < 0 || target > Program.Count) throw new ArgumentOutOfRangeException(nameof(target));
                            Program.PatchTarget(index, target);
                        },
                        (message, field) => throw Error("JSON_CUSTOM_NODE", message, field is null ? path : Field(obj!, field, path, false).Path));
                    try { registration.Compiler!(context); }
                    catch (VmCompileFault) { throw; }
                    catch (Exception error) when (error is not OutOfMemoryException)
                    { throw Error("NODE_COMPILER_ERROR", $"Compiler for '{definition.Operation}' failed: {error.Message}", path); }
                    finally { context.Close(); }
                    break;
                }
            }
            Emit(OpcodeId.ExitNode, node, node);
        }

        private DynValue Literal(JsonNode? json, string path)
        {
            if (json is null) return DynValue.Null;
            if (json is JsonValue value)
            {
                if (value.TryGetValue<int>(out int integer)) return DynValue.FromInt(integer);
                if (value.TryGetValue<float>(out float number)) return DynValue.FromFloat(number);
                if (value.TryGetValue<bool>(out bool boolean)) return DynValue.FromBool(boolean);
                if (value.TryGetValue<string>(out string? text)) return DynValue.FromString(text!);
            }
            if (json is JsonArray array)
                return DynValue.FromArray(array.Select((item, i) => Literal(item, LocatedJson.Child(path, i.ToString()))).ToList());
            if (json is JsonObject obj)
            {
                if (obj.TryGetPropertyValue("$type", out var tag))
                {
                    string type = Text(tag, LocatedJson.Child(path, "$type")).ToLowerInvariant();
                    float Number(string key, float? fallback = null)
                    {
                        if (!obj.ContainsKey(key) && fallback.HasValue) return fallback.Value;
                        var field = Field(obj, key, path);
                        var scalar = Literal(field.Value, field.Path);
                        if (scalar.Type == ValueType.Int) return scalar.AsInt();
                        if (scalar.Type == ValueType.Float) return scalar.AsFloat();
                        throw Error("JSON_TYPE", $"Component '{key}' must be numeric.", field.Path);
                    }
                    int Integer(string key)
                    {
                        var field = Field(obj, key, path);
                        var scalar = Literal(field.Value, field.Path);
                        if (scalar.Type == ValueType.Int) return scalar.AsInt();
                        throw Error("JSON_TYPE", $"Component '{key}' must be Int32.", field.Path);
                    }
                    return type switch
                    {
                        "vector2" => DynValue.FromVector2(new(Number("x"), Number("y"))),
                        "vector2i" => DynValue.FromVector2i(new(Integer("x"), Integer("y"))),
                        "vector3" => DynValue.FromVector3(new(Number("x"), Number("y"), Number("z"))),
                        "vector3i" => DynValue.FromVector3i(new(Integer("x"), Integer("y"), Integer("z"))),
                        "color" => DynValue.FromColor(new(Number("r"), Number("g"), Number("b"), Number("a", 1))),
                        _ => throw Error("JSON_UNKNOWN_TYPE", $"Unknown literal type '{type}'.", path)
                    };
                }
                return DynValue.FromDictionary(obj.ToDictionary(pair => DynValue.FromString(pair.Key),
                    pair => Literal(pair.Value, LocatedJson.Child(path, pair.Key))));
            }
            throw Error("JSON_TYPE", "Unsupported literal value.", path);
        }
    }
}
