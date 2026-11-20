#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using VMRuntime.Diagnostics;
using VMRuntime.Logging;
namespace VMRuntime.GodotApi;

/// <summary>Godot entry point. All public arguments/results are usable from GDScript.</summary>
[GlobalClass]
public partial class VmEngineNode : Node
{
    private readonly VmEngine _engine;
    /// <summary>C#-only access for advanced node compiler registration; use the typed methods from GDScript.</summary>
    public VmEngine CoreEngine => _engine;
    private readonly GodotVmLogger _logger = new();
    /// <summary>Controls automatic Godot error logging; returned diagnostics are always retained.</summary>
    public bool LogErrors { get => _logger.Enabled; set => _logger.Enabled = value; }
    /// <summary>Creates a ready-to-use engine with Godot object access and error logging.</summary>
    public VmEngineNode()
    {
        Initializer.Initialize();
        _engine = new(_logger, new GodotHostObjects());
    }

    /// <summary>Compiles JSON or reuses the unchanged cached program; no execution occurs.</summary>
    public VmCompilationResultObject CompileJson(string json, string sourceFile, string scriptId)
        => VmCompilationResultObject.Wrap(_engine.CompileJson(json, sourceFile, string.IsNullOrEmpty(scriptId) ? null : scriptId));
    /// <summary>Compiles JSON with a logical source filename.</summary>
    public VmCompilationResultObject CompileJson(string json, string sourceFile) => CompileJson(json, sourceFile, "");
    /// <summary>Compiles an in-memory JSON document.</summary>
    public VmCompilationResultObject CompileJson(string json) => CompileJson(json, "<json>", "");

    /// <summary>Compiles only a selected parsed JSON expression, such as item["onAttack"], with supplied metadata.</summary>
    /// <param name="fragment">JSON-native dictionary, array, scalar or nil. A string is a literal, not JSON text.</param>
    /// <param name="metadata">Logical root, identifier and optional per-node original locations.</param>
    /// <returns>A compiled handle or typed compilation error; no containing item is compiled.</returns>
    public VmCompilationResultObject CompileFragment(Variant fragment, VmCompilationMetadataObject metadata)
    {
        VmCompilationMetadata context;
        try
        {
            ArgumentNullException.ThrowIfNull(metadata);
            context = metadata.ToCore();
        }
        catch (ArgumentException error) { return FragmentFailure("INVALID_METADATA", error, metadata); }
        try
        {
            return VmCompilationResultObject.Wrap(_engine.CompileFragment(GodotJsonFragment.Convert(fragment), context));
        }
        catch (Exception error) when (error is ArgumentException or InvalidOperationException or NotSupportedException)
        { return FragmentFailure("JSON_INPUT_TYPE", error, metadata); }
    }

    /// <summary>Compiles fragment JSON text with merger metadata; no file is read.</summary>
    public VmCompilationResultObject CompileFragmentJson(string json, VmCompilationMetadataObject metadata)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(metadata);
            return VmCompilationResultObject.Wrap(_engine.CompileFragmentJson(json, metadata.ToCore()));
        }
        catch (ArgumentException error) { return FragmentFailure("INVALID_METADATA", error, metadata); }
    }

    private VmCompilationResultObject FragmentFailure(string code, Exception error, VmCompilationMetadataObject? metadata)
    {
        var diagnostic = new VmCompilationError(code, error.Message,
            new(string.IsNullOrWhiteSpace(metadata?.SourceFile) ? "<fragment>" : metadata.SourceFile,
                metadata?.RootPath ?? "", 0, 0));
        _logger.Log(diagnostic);
        return VmCompilationResultObject.Wrap(VmCompilationResult.Failed(diagnostic));
    }

    /// <summary>Reads res://, user:// or an absolute path using Godot, then compiles if changed.</summary>
    public VmCompilationResultObject LoadAndCompile(string path, string scriptId)
    {
        using var file = Godot.FileAccess.Open(path, Godot.FileAccess.ModeFlags.Read);
        if (file is null)
        {
            var error = new VmCompilationError("SOURCE_IO", $"Cannot open '{path}': {Godot.FileAccess.GetOpenError()}.", new(path));
            _logger.Log(error);
            return VmCompilationResultObject.Wrap(VmCompilationResult.Failed(error));
        }
        return CompileJson(file.GetAsText(), path, scriptId);
    }
    /// <summary>Loads and compiles a source using its path as identifier.</summary>
    public VmCompilationResultObject LoadAndCompile(string path) => LoadAndCompile(path, "");

    /// <summary>Executes a reusable handle with Godot-native variables and typed per-call options.</summary>
    /// <param name="script">Handle obtained from a successful compilation result's Script property.</param>
    /// <param name="variables">Named values for this invocation; null means an empty context.</param>
    /// <param name="options">Optional VmExecutionOptionsObject; explanation defaults to false.</param>
    /// <returns>A typed result with Value, Error and optional Explanation.</returns>
    public VmExecutionResultObject Execute(VmCompiledScriptObject script,
        Godot.Collections.Dictionary? variables, VmExecutionOptionsObject? options)
    {
        try
        {
            if (script?.Data is null) throw new ArgumentException("A valid compiled script is required.");
            var context = new Dictionary<string, DynValue>(StringComparer.Ordinal);
            if (variables is not null)
            {
                foreach (var pair in variables)
                {
                    string name = pair.Key.VariantType switch
                    {
                        Variant.Type.String => pair.Key.AsString(),
                        Variant.Type.StringName => pair.Key.AsStringName().ToString(),
                        _ => throw new ArgumentException("Variable names must be strings or StringNames.")
                    };
                    try { context.Add(name, DynValueConverter.FromVariant(pair.Value)); }
                    catch (Exception conversion) when (conversion is InvalidCastException or OverflowException or NotSupportedException or InvalidOperationException)
                    {
                        throw new ArgumentException($"Variable '{name}' ({pair.Value.VariantType}) cannot be converted to a VM value: {conversion.Message}",
                            nameof(variables), conversion);
                    }
                }
            }
            var result = _engine.Execute(script.Data, context, options?.ToCore());
            try { return VmExecutionResultObject.Wrap(result); }
            catch (Exception conversion) when (conversion is InvalidOperationException or InvalidCastException or NotSupportedException or OverflowException)
            {
                var inputs = new Dictionary<string, VmValueSnapshot> { ["value"] = VmValueSnapshot.Capture(result.Value) };
                var error = new VmExecutionError("RESULT_CONVERSION", conversion.Message, script.Data.GetInstructionSource(0),
                    new("return_to_godot", inputs, script.Data.RootPath), new(-1, default, inputs));
                _logger.Log(error);
                return VmExecutionResultObject.Wrap(VmExecutionResult.Failed(error, result.Explanation));
            }
        }
        catch (Exception exception) when (exception is ArgumentException or InvalidCastException or OverflowException or NotSupportedException or InvalidOperationException)
        {
            var error = new VmExecutionError("INVALID_ARGUMENT", exception.Message,
                script?.Data?.GetInstructionSource(0) ?? new("<execute>", "", 0, 0),
                new("execute", new Dictionary<string, VmValueSnapshot>(), script?.Data?.RootPath ?? ""),
                new(-1, default, new Dictionary<string, VmValueSnapshot>()));
            _logger.Log(error);
            return VmExecutionResultObject.Wrap(VmExecutionResult.Failed(error));
        }
    }
    /// <summary>Executes with supplied variables and explanation disabled.</summary>
    public VmExecutionResultObject Execute(VmCompiledScriptObject script, Godot.Collections.Dictionary variables)
        => Execute(script, variables, null);
    /// <summary>Executes with an empty variable context and explanation disabled.</summary>
    public VmExecutionResultObject Execute(VmCompiledScriptObject script) => Execute(script, null, null);

    /// <summary>Registers a C#/GDScript callable. The callback receives positional script arguments.</summary>
    public void RegisterFunction(string name, Callable callback)
    {
        _engine.RegisterFunction(name, args =>
        {
            if (callback.Target is { } target && !GodotObject.IsInstanceValid(target))
                throw new InvalidOperationException($"Callable '{name}' target has been freed.");
            return DynValueConverter.FromVariant(callback.Call(args.Select(DynValueConverter.ToVariant).ToArray()));
        });
    }

    /// <summary>Registers a documented project node. The callable receives dictionaries (inputs, parameters).</summary>
    public VmNodeRegistrationResultObject RegisterNode(VmNodeDefinitionObject definition, Callable callback)
        => RegisterNode(definition, callback, false);

    /// <summary>Registers or explicitly replaces a project callback node; built-ins remain reserved.</summary>
    public VmNodeRegistrationResultObject RegisterNode(VmNodeDefinitionObject definition, Callable callback, bool replace)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(definition);
            var schema = definition.ToCore();
            var inputLayout = schema.Inputs.ToArray();
            _engine.RegisterNode(schema, args =>
            {
                if (callback.Target is { } target && !GodotObject.IsInstanceValid(target))
                    throw new InvalidOperationException("Node callback target has been freed.");
                var inputs = new Godot.Collections.Dictionary();
                foreach (var input in inputLayout)
                {
                    if (input.Variadic)
                    {
                        var values = new Godot.Collections.Array();
                        foreach (var value in args.GetInputs(input.Name)) values.Add(DynValueConverter.ToVariant(value));
                        inputs[input.Name] = values;
                    }
                    else inputs[input.Name] = DynValueConverter.ToVariant(args.GetInput(input.Name));
                }
                var parameters = new Godot.Collections.Dictionary();
                foreach (var parameter in args.Parameters) parameters[parameter.Key] = DynValueConverter.ToVariant(parameter.Value);
                return DynValueConverter.FromVariant(callback.Call(inputs, parameters));
            }, replace);
            return new() { Data = new() };
        }
        catch (Exception error) when (error is ArgumentException or InvalidOperationException or System.Text.Json.JsonException)
        {
            var diagnostic = new VmCompilationError("NODE_REGISTRATION", error.Message, new("<node-registry>", "", 0, 0));
            _logger.Log(diagnostic);
            return new() { Data = new(diagnostic) };
        }
    }

    /// <summary>Returns the typed catalog of JSON nodes currently available in this project.</summary>
    public VmNodeCatalogObject GetNodeCatalog() => new() { Data = _engine.GetNodeCatalog() };
    /// <summary>Exports the node catalog as JSON for a website or editor.</summary>
    public string GetNodeCatalogJson() => _engine.GetNodeCatalogJson();
    /// <summary>Exports the catalog with explicit indentation.</summary>
    public string GetNodeCatalogJson(bool indented) => _engine.GetNodeCatalogJson(indented);

    /// <summary>Clears compilation cache entries without invalidating handles already held by the game.</summary>
    public void ClearCache() => _engine.ClearCache();

    private sealed class GodotVmLogger : IVmLogger
    {
        internal bool Enabled = true;
        public void Log(VmError error) { if (Enabled) GD.PushError(error.ToJson(false)); }
    }
}
