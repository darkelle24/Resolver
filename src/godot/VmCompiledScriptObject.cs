#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
namespace VMRuntime.GodotApi;

/// <summary>Reusable compiled handle. Execute never reads its source file.</summary>
[GlobalClass]
public partial class VmCompiledScriptObject : RefCounted
{
    internal CompiledScript? Data;
    public string Id => Data?.Id ?? "";
    public string SourceFile => Data?.SourceFile ?? "";
    public string SourceHash => Data?.SourceHash ?? "";
    public string RootPath => Data?.RootPath ?? "";
    public string MetadataHash => Data?.MetadataHash ?? "";
    public bool IsFragment => Data?.IsFragment ?? false;
    public long RegistryVersion => Data?.RegistryVersion ?? 0;
    public string RegistryHash => Data?.RegistryHash ?? "";
    public VmCompilationMetadataObject? Metadata => Data is null ? null : VmCompilationMetadataObject.Wrap(Data.Metadata);
    public int InstructionCount => Data?.InstructionCount ?? 0;
    /// <summary>Returns the original source text, preserving its formatting.</summary>
    public string GetSourceJson() => Data?.OriginalJson ?? "";
    /// <summary>Gets the frozen catalog used to compile this handle.</summary>
    public VmNodeCatalogObject? GetNodeCatalog() => Data is null ? null : new() { Data = Data.NodeCatalog };
    /// <summary>Prints the exact catalog associated with this compiled program.</summary>
    public string GetNodeCatalogJson() => GetNodeCatalogJson(true);
    /// <summary>Prints the associated catalog with explicit indentation.</summary>
    public string GetNodeCatalogJson(bool indented) => Data?.GetNodeCatalogJson(indented) ?? "null";
    /// <summary>Exports bytecode and source bindings for inspection.</summary>
    public string ExportProgramJson() => ExportProgramJson(true);
    /// <summary>Serializes with an explicit indentation option.</summary>
    public string ExportProgramJson(bool indented) => Data?.ExportProgramJson(indented) ?? "null";
    /// <summary>Gets the original JSON position associated with an instruction index.</summary>
    public VmSourceLocationObject? GetInstructionSource(int index)
        => Data is not null && index >= 0 && index < Data.InstructionCount
            ? VmSourceLocationObject.Wrap(Data.GetInstructionSource(index)) : null;
    internal static VmCompiledScriptObject Wrap(CompiledScript data) => new() { Data = data };
}
