#nullable enable
using Godot;
using VMRuntime.Diagnostics;
namespace VMRuntime.GodotApi;

/// <summary>Editable original-source position supplied by a JSON merger.</summary>
[GlobalClass]
public partial class VmSourceOriginObject : RefCounted
{
    /// <summary>Original contributing file or source identifier.</summary>
    public string File { get; set; } = "<merged>";
    /// <summary>RFC 6901 pointer in that original file, which may differ from the merged path.</summary>
    public string Path { get; set; } = "";
    /// <summary>One-based original line, or zero when unavailable.</summary>
    public long Line { get; set; }
    /// <summary>One-based UTF-8 byte column, or zero when unavailable.</summary>
    public long ByteColumn { get; set; }
    /// <summary>Serializes this source position.</summary>
    public string ToJson() => ToJson(true);
    /// <summary>Serializes with explicit indentation.</summary>
    public string ToJson(bool indented) => ToCore().ToJson(indented);
    internal VmSourceLocation ToCore() => new(File, Path, Line, ByteColumn);
    internal static VmSourceOriginObject Wrap(VmSourceLocation data)
        => new() { File = data.File, Path = data.Path, Line = data.Line, ByteColumn = data.ByteColumn };
}
