using System.Collections.ObjectModel;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using VMRuntime.Diagnostics;

namespace VMRuntime;

/// <summary>Project-scoped registry used by both the compiler and the JSON documentation exporter.</summary>
public sealed class VmNodeRegistry
{
    private readonly object _gate = new();
    private readonly Dictionary<string, VmNodeRegistration> _nodes = new(StringComparer.Ordinal);
    private long _version;
    private VmNodeRegistrySnapshot? _snapshot;
    /// <summary>Creates a registry containing all supported built-in JSON nodes.</summary>
    public VmNodeRegistry() => VmBuiltinNodes.Register(this);
    /// <summary>Registers an eager node and its execution callback; replacement must be explicit.</summary>
    public void Register(VmNodeDefinition definition, Func<VmNodeArguments, DynValue> callback, bool replace = false)
    {
        ArgumentNullException.ThrowIfNull(definition);
        ArgumentNullException.ThrowIfNull(callback);
        Add(new(definition.Snapshot("eager", "project"), VmNodeKind.Callback, null, callback, null), replace, false);
    }
    /// <summary>Registers one class containing a node's documentation and eager implementation.</summary>
    public void Register(IVmJsonNode node, bool replace = false)
    { ArgumentNullException.ThrowIfNull(node); Register(node.Definition, node.Execute, replace); }
    /// <summary>Registers a custom C# compiler, allowing lazy branches without editing the central compiler.</summary>
    public void RegisterCompiler(VmNodeDefinition definition, Action<VmNodeCompilationContext> compiler, bool replace = false)
    {
        ArgumentNullException.ThrowIfNull(definition);
        ArgumentNullException.ThrowIfNull(compiler);
        Add(new(definition.Snapshot(definition.Evaluation == "eager" ? "custom" : definition.Evaluation, "project"), VmNodeKind.CustomCompiler, null, null, compiler), replace, false);
    }
    /// <summary>Registers a class defining a custom node and its source-aware compiler.</summary>
    public void RegisterCompiler(IVmJsonNodeCompiler node, bool replace = false)
    { ArgumentNullException.ThrowIfNull(node); RegisterCompiler(node.Definition, node.Compile, replace); }
    /// <summary>Returns a detached, versioned description of the nodes available in this registry.</summary>
    public VmNodeCatalog GetCatalog() => Snapshot().Catalog;
    /// <summary>Exports the catalog as JSON for an editor or website.</summary>
    public string GetCatalogJson(bool indented = true) => GetCatalog().ToJson(indented);

    internal void Builtin(VmNodeDefinition definition, VmNodeKind kind, OpcodeId? opcode = null)
        => Add(new(definition.Snapshot(origin: "builtin"), kind, opcode, null, null), false, true);
    internal void BuiltinCompiler(VmNodeDefinition definition, Action<VmNodeCompilationContext> compiler)
        => Add(new(definition.Snapshot(origin: "builtin"), VmNodeKind.CustomCompiler, null, null, compiler), false, true);
    private void Add(VmNodeRegistration registration, bool replace, bool builtin)
    {
        ArgumentNullException.ThrowIfNull(registration.Definition);
        foreach (var value in registration.Definition.Inputs.Select(input => input.DefaultValue)
            .Concat(registration.Definition.Parameters.Select(parameter => parameter.DefaultValue)).Where(value => value.HasValue))
        {
            try { _ = new LocatedJson(value!.Value.GetRawText(), "<node-definition>"); }
            catch (VmCompileFault error) { throw new ArgumentException("Invalid node default: " + error.Error.Message); }
        }
        lock (_gate)
        {
            string id = registration.Definition.Operation;
            if (_nodes.ContainsKey(id) && !replace) throw new ArgumentException($"Node '{id}' is already registered.");
            if (_nodes.TryGetValue(id, out var existing) && existing.Definition.Origin == "builtin")
                throw new ArgumentException($"Built-in node '{id}' cannot be replaced; choose a project operation name.");
            if (_nodes.Values.Any(node => node.Definition.Operation != id && node.Definition.Aliases.Contains(id)))
                throw new ArgumentException($"Operation '{id}' collides with an existing alias.");
            var proposedNames = registration.Definition.Aliases.Append(id).ToHashSet(StringComparer.Ordinal);
            foreach (var node in _nodes.Values.Where(node => node.Definition.Operation != id))
                if (node.Definition.Aliases.Append(node.Definition.Operation).Any(proposedNames.Contains))
                    throw new ArgumentException($"Aliases for '{id}' collide with node '{node.Definition.Operation}'.");
            _nodes[id] = registration;
            if (!builtin) _version++;
            _snapshot = null;
        }
    }
    internal VmNodeRegistrySnapshot Snapshot()
    {
        lock (_gate)
        {
            if (_snapshot is not null) return _snapshot;
            var nodes = _nodes.Values.OrderBy(node => node.Definition.Operation, StringComparer.Ordinal).ToArray();
            var definitions = nodes.Select(node => node.Definition).ToArray();
            string json = JsonSerializer.Serialize(new { FormatVersion = 1, Syntax = new VmNodeSyntax(), ValueTypes = VmNodeTypes.All,
                Nodes = definitions, Literals = VmBuiltinNodes.LiteralTypes }, VmJson.Options(false));
            string hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(json))).ToLowerInvariant();
            var names = new Dictionary<string, VmNodeRegistration>(StringComparer.Ordinal);
            foreach (var node in nodes)
                foreach (var name in node.Definition.Aliases.Append(node.Definition.Operation)) names.Add(name, node);
            return _snapshot = new(new ReadOnlyDictionary<string, VmNodeRegistration>(names), new(_version, hash, definitions));
        }
    }
}

internal enum VmNodeKind { Opcode, Constant, Variable, If, And, Or, While, Array, Sequence, Call, ObjectCall, Callback, CustomCompiler }
internal sealed record VmNodeRegistration(VmNodeDefinition Definition, VmNodeKind Kind, OpcodeId? Opcode,
    Func<VmNodeArguments, DynValue>? Callback, Action<VmNodeCompilationContext>? Compiler);
internal sealed record VmNodeRegistrySnapshot(IReadOnlyDictionary<string, VmNodeRegistration> Names, VmNodeCatalog Catalog)
{
    internal VmNodeRegistration? Find(string name) => Names.GetValueOrDefault(name.ToLowerInvariant());
}
