using VMRuntime;
using VMRuntime.Diagnostics;
using VMRuntime.Logging;
using System.Text.Json.Nodes;
using System.Reflection;

namespace VM.RuleNodeSystem;

/// <summary>Compatibility entry point; new callers should use VmEngine for typed compilation results.</summary>
public static class RuleNodeProcessor
{
    /// <summary>Compiles built-in JSON expressions and appends source-mapped bytecode to an assembler.</summary>
    /// <param name="json">Source JSON.</param>
    /// <param name="assembler">Destination assembler.</param>
    /// <param name="sourceFile">Logical filename used in diagnostics.</param>
    /// <param name="logger">Optional structured error destination.</param>
    /// <exception cref="VmCompilationException">JSON compilation failed; Error contains the typed diagnostic.</exception>
    public static void ProcessJson(string json, Assembler assembler, string sourceFile = "<json>", IVmLogger? logger = null)
    {
        ArgumentNullException.ThrowIfNull(assembler);
        // Keep explicitly registered legacy rule compilers working at this compatibility entry point.
        // The source-aware VmEngine API deliberately uses the new built-in compiler.
        try
        {
            var source = new LocatedJson(json, sourceFile);
            if (source.Root is JsonObject obj && obj["type"] is JsonValue tag &&
                tag.TryGetValue<string>(out var typeName) && typeName is not null && RuleNodeFactory.IsRegistered(typeName))
            {
                var root = RuleNodeFactory.CreateFromJson(obj, "$");
                var validation = new ValidationContext();
                ValidateLegacy(root, validation);
                if (validation.HasErrors)
                    throw new RuleParseException(string.Join(Environment.NewLine,
                        validation.Errors.Select(error => error.Path + ": " + error.Message)));
                root.Compile(assembler);
                return;
            }
        }
        catch (VmCompileFault fault)
        {
            logger?.Log(fault.Error);
            throw new VmCompilationException(fault.Error);
        }
        catch (RuleParseException error)
        {
            var diagnostic = new VmCompilationError("LEGACY_RULE_PARSE", error.Message, new(sourceFile));
            logger?.Log(diagnostic);
            throw new VmCompilationException(diagnostic);
        }
        var result = new VmJsonCompiler().Compile(json, sourceFile);
        if (result.Error is not null)
        {
            logger?.Log(result.Error);
            throw new VmCompilationException(result.Error);
        }
        assembler.Append(result.Script!);
    }

    private static void ValidateLegacy(RuleNode node, ValidationContext context)
    {
        node.Validate(context);
        foreach (var property in node.GetType().GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
        {
            if (property.GetCustomAttribute<InputFieldAttribute>() is not null && property.GetValue(node) is RuleNode child)
                ValidateLegacy(child, context);
            if (property.GetCustomAttribute<VariadicInputAttribute>() is not null && property.GetValue(node) is IReadOnlyList<RuleNode> children)
                foreach (var item in children) ValidateLegacy(item, context);
        }
    }
}

/// <summary>Exception bridge for legacy callers; the new engine returns diagnostics instead.</summary>
public sealed class VmCompilationException : RuleParseException
{
    public VmCompilationError Error { get; }
    /// <summary>Creates an exception retaining the structured compilation diagnostic.</summary>
    public VmCompilationException(VmCompilationError error) : base(error.Message) => Error = error;
}
