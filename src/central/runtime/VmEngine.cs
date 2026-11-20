using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using VMRuntime.Diagnostics;
using VMRuntime.Logging;

namespace VMRuntime;

/// <summary>Public engine API: compile/cache once, then execute with independent per-call variables.</summary>
public sealed class VmEngine
{
    private readonly IVmLogger _logger;
    private readonly IVmHostObjects _host;
    private readonly object _gate = new();
    private readonly Dictionary<(string File, string Id, string RootPath, bool Fragment), CompiledScript> _cache = new();
    private readonly Dictionary<string, Func<IReadOnlyList<DynValue>, DynValue>> _functions = new(StringComparer.Ordinal);
    private readonly VmJsonCompiler _compiler;
    /// <summary>Registry shared by compilation and catalog export for this engine instance.</summary>
    public VmNodeRegistry Nodes { get; }

    /// <summary>Creates a ready-to-use engine; logging and object access are injected by the host.</summary>
    public VmEngine(IVmLogger? logger = null, IVmHostObjects? host = null, VmNodeRegistry? nodes = null)
    {
        (_logger, _host) = (logger ?? NullVmLogger.Instance, host ?? new UnavailableHostObjects());
        Nodes = nodes ?? new();
        _compiler = new(Nodes);
    }

    /// <summary>Compiles JSON or returns the cached program when its source hash is unchanged.</summary>
    public VmCompilationResult CompileJson(string json, string sourceFile = "<json>", string? scriptId = null)
    {
        ArgumentNullException.ThrowIfNull(json);
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceFile);
        string id = scriptId ?? sourceFile;
        string hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(json))).ToLowerInvariant();
        VmCompilationResult result;
        lock (_gate)
        {
            var nodes = Nodes.Snapshot();
            if (_cache.TryGetValue((sourceFile, id, "", false), out var cached) && cached.SourceHash == hash && cached.RegistryVersion == nodes.Catalog.RegistryVersion) return new(cached, null);
            result = _compiler.Compile(json, sourceFile, id, nodes);
            if (result.Script is not null) _cache[(sourceFile, id, "", false)] = result.Script;
        }
        if (result.Error is not null) _logger.Log(result.Error);
        return result;
    }

    /// <summary>Compiles only a selected parsed expression, such as item["onAttack"], with merger metadata.</summary>
    /// <param name="fragment">The selected JsonNode; containing/sibling data is not inspected.</param>
    /// <param name="metadata">Identifier, logical root and optional original source map supplied by the caller.</param>
    /// <returns>A cached or newly compiled handle, or a typed compilation failure.</returns>
    public VmCompilationResult CompileFragment(JsonNode? fragment, VmCompilationMetadata metadata)
    {
        ArgumentNullException.ThrowIfNull(metadata);
        try { return CompileFragmentJson(VmFragmentJson.Serialize(fragment), metadata); }
        catch (Exception error) when (error is ArgumentException or InvalidOperationException or JsonException)
        { return ReportFragmentFailure(error, metadata); }
    }

    /// <summary>Compiles a selected JsonElement without compiling the document that contains it.</summary>
    public VmCompilationResult CompileFragment(JsonElement fragment, VmCompilationMetadata metadata)
    {
        ArgumentNullException.ThrowIfNull(metadata);
        try { return CompileFragmentJson(fragment.GetRawText(), metadata); }
        catch (InvalidOperationException error) { return ReportFragmentFailure(error, metadata); }
    }

    /// <summary>Compiles supplied fragment text with merger metadata; no file is opened.</summary>
    public VmCompilationResult CompileFragmentJson(string json, VmCompilationMetadata metadata)
    {
        ArgumentNullException.ThrowIfNull(json);
        ArgumentNullException.ThrowIfNull(metadata);
        VmCompilationMetadata context;
        try { context = metadata.Snapshot(); }
        catch (ArgumentException error) { return ReportFragmentFailure(error, metadata, "INVALID_METADATA"); }
        string hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(json))).ToLowerInvariant();
        string metadataHash = context.Fingerprint();
        var key = (context.SourceFile, context.EffectiveId, context.RootPath, true);
        VmCompilationResult result;
        lock (_gate)
        {
            var nodes = Nodes.Snapshot();
            if (_cache.TryGetValue(key, out var cached) && cached.SourceHash == hash && cached.MetadataHash == metadataHash && cached.RegistryVersion == nodes.Catalog.RegistryVersion)
                return new(cached, null);
            result = _compiler.CompileFragmentJson(json, context, nodes);
            if (result.Script is not null) _cache[key] = result.Script;
        }
        if (result.Error is not null) _logger.Log(result.Error);
        return result;
    }

    private VmCompilationResult ReportFragmentFailure(Exception error, VmCompilationMetadata metadata, string code = "JSON_INPUT_TYPE")
    {
        var result = VmJsonCompiler.InvalidFragment(error, metadata, code);
        _logger.Log(result.Error!);
        return result;
    }

    /// <summary>Reads a filesystem file and compiles it if changed. Execute performs no file I/O.</summary>
    public VmCompilationResult LoadAndCompile(string path, string? scriptId = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        try { return CompileJson(File.ReadAllText(path), path, scriptId); }
        catch (Exception error) when (error is IOException or UnauthorizedAccessException)
        {
            var diagnostic = new VmCompilationError("SOURCE_IO", error.Message, new(path));
            _logger.Log(diagnostic);
            return new(null, diagnostic);
        }
    }

    /// <summary>Executes a compiled handle without parsing JSON, using a fresh stack and variable map.</summary>
    /// <param name="script">Reusable compiled program returned by CompileJson or LoadAndCompile.</param>
    /// <param name="variables">Variables for this invocation; collections/host objects retain reference semantics.</param>
    /// <param name="options">Per-call explanation and execution limits.</param>
    /// <returns>A typed result containing a value or a detailed error, plus an optional trace.</returns>
    public VmExecutionResult Execute(CompiledScript script, IReadOnlyDictionary<string, DynValue>? variables = null,
        VmExecutionOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(script);
        options ??= new();
        options.Validate();
        Dictionary<string, Func<IReadOnlyList<DynValue>, DynValue>> functions;
        lock (_gate) functions = new(_functions, StringComparer.Ordinal);
        var context = variables is null ? new Dictionary<string, DynValue>()
            : new Dictionary<string, DynValue>(variables, StringComparer.Ordinal);
        var result = new VM(script.Bytecode, script, context, _host, functions, options).Execute();
        if (result.Error is not null) _logger.Log(result.Error);
        return result;
    }

    /// <summary>Registers or replaces an explicitly exposed C#/Lua host callback.</summary>
    public void RegisterFunction(string name, Func<IReadOnlyList<DynValue>, DynValue> callback)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(callback);
        lock (_gate) _functions[name] = callback;
    }

    /// <summary>Registers a documented project node with eagerly evaluated inputs.</summary>
    public void RegisterNode(VmNodeDefinition definition, Func<VmNodeArguments, DynValue> implementation, bool replace = false)
        => Nodes.Register(definition, implementation, replace);
    /// <summary>Registers a class providing both a node definition and its implementation.</summary>
    public void RegisterNode(IVmJsonNode node, bool replace = false) => Nodes.Register(node, replace);
    /// <summary>Registers an advanced C# node controlling its own input evaluation and bytecode.</summary>
    public void RegisterCompilerNode(IVmJsonNodeCompiler node, bool replace = false) => Nodes.RegisterCompiler(node, replace);
    /// <summary>Gets the typed catalog of JSON nodes registered in this project.</summary>
    public VmNodeCatalog GetNodeCatalog() => Nodes.GetCatalog();
    /// <summary>Exports the current node catalog as JSON for a website or editor.</summary>
    public string GetNodeCatalogJson(bool indented = true) => Nodes.GetCatalogJson(indented);

    /// <summary>Removes cached handles; already returned compiled scripts remain executable.</summary>
    public void ClearCache() { lock (_gate) _cache.Clear(); }
}
