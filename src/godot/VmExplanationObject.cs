#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using VMRuntime.Diagnostics;
namespace VMRuntime.GodotApi;

/// <summary>Source-linked explanation for one invocation.</summary>
[GlobalClass]
public partial class VmExplanationObject : RefCounted
{
    internal VmExplanation? Data;
    public int FormatVersion => Data?.FormatVersion ?? 1;
    public string Script => Data?.Script ?? "";
    public string SourceFile => Data?.SourceFile ?? "";
    public string SourceHash => Data?.SourceHash ?? "";
    public string RootPath => Data?.RootPath ?? "";
    public string MetadataHash => Data?.MetadataHash ?? "";
    public bool IsFragment => Data?.IsFragment ?? false;
    public long RegistryVersion => Data?.RegistryVersion ?? 0;
    public string RegistryHash => Data?.RegistryHash ?? "";
    public bool Success => Data?.Success ?? false;
    public bool Truncated => Data?.Truncated ?? false;
    public VmTraceNodeObject? Tree => Data?.Tree is { } tree ? VmTraceNodeObject.Wrap(tree) : null;
    /// <summary>Prints the portable explanation consumed by a web visualizer.</summary>
    public string ToJson() => ToJson(true);
    /// <summary>Serializes with an explicit indentation option.</summary>
    public string ToJson(bool indented) => Data?.ToJson(indented) ?? "null";
    internal static VmExplanationObject Wrap(VmExplanation data) => new() { Data = data };
}
