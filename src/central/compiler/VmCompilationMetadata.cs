using System.Collections.ObjectModel;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using VMRuntime.Diagnostics;

namespace VMRuntime;

/// <summary>Context supplied by a data merger when compiling a selected JSON fragment.</summary>
public sealed class VmCompilationMetadata : VmJsonObject
{
    /// <summary>Optional script identifier, for example items/sword:onAttack.</summary>
    public string? ScriptId { get; init; }
    /// <summary>Fallback source identifier; no file is opened by fragment compilation.</summary>
    public string SourceFile { get; init; } = "<merged>";
    /// <summary>JSON Pointer of the fragment in the merged document, for example /items/sword/onAttack.</summary>
    public string RootPath { get; init; } = "";
    /// <summary>Optional original locations keyed by JSON Pointers relative to the fragment.</summary>
    public IReadOnlyDictionary<string, VmSourceLocation> SourceMap { get; init; }
        = new ReadOnlyDictionary<string, VmSourceLocation>(new Dictionary<string, VmSourceLocation>());
    /// <summary>Application labels such as item_id, mod_id or category; retained in the compiled handle.</summary>
    public IReadOnlyDictionary<string, string> Tags { get; init; }
        = new ReadOnlyDictionary<string, string>(new Dictionary<string, string>());

    internal string EffectiveId => ScriptId ?? SourceFile + (RootPath.Length == 0 ? "" : "#" + RootPath);
    internal VmCompilationMetadata Snapshot()
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(SourceFile);
        if (ScriptId is not null) ArgumentException.ThrowIfNullOrWhiteSpace(ScriptId);
        ValidatePointer(RootPath, nameof(RootPath));
        ArgumentNullException.ThrowIfNull(SourceMap);
        ArgumentNullException.ThrowIfNull(Tags);
        var origins = new Dictionary<string, VmSourceLocation>(StringComparer.Ordinal);
        foreach (var pair in SourceMap)
        {
            ValidatePointer(pair.Key, nameof(SourceMap));
            ArgumentNullException.ThrowIfNull(pair.Value);
            ArgumentException.ThrowIfNullOrWhiteSpace(pair.Value.File);
            ValidatePointer(pair.Value.Path, nameof(SourceMap));
            if (pair.Value.Line < 0 || pair.Value.ByteColumn < 0 ||
                ((pair.Value.Line == 0) != (pair.Value.ByteColumn == 0)))
                throw new ArgumentException("Source positions must both be zero (unknown), or both be positive.", nameof(SourceMap));
            origins.Add(pair.Key, pair.Value);
        }
        var tags = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var pair in Tags)
        {
            ArgumentNullException.ThrowIfNull(pair.Key);
            ArgumentNullException.ThrowIfNull(pair.Value);
            tags.Add(pair.Key, pair.Value);
        }
        return new()
        {
            ScriptId = ScriptId, SourceFile = SourceFile, RootPath = RootPath,
            SourceMap = new ReadOnlyDictionary<string, VmSourceLocation>(origins),
            Tags = new ReadOnlyDictionary<string, string>(tags)
        };
    }
    internal static void ValidatePointer(string pointer, string argument)
    {
        ArgumentNullException.ThrowIfNull(pointer, argument);
        if (pointer.Length > 0 && pointer[0] != '/')
            throw new ArgumentException("Expected an RFC 6901 JSON Pointer: empty, or beginning with '/'.", argument);
        for (int i = 0; i < pointer.Length; i++)
            if (pointer[i] == '~' && (++i == pointer.Length || pointer[i] is not ('0' or '1')))
                throw new ArgumentException("JSON Pointer '~' escapes must be '~0' or '~1'.", argument);
    }
    internal VmSourceLocation Resolve(string relativePath)
    {
        string ancestor = relativePath;
        while (true)
        {
            if (SourceMap.TryGetValue(ancestor, out var source))
            {
                if (ancestor == relativePath) return source;
                // An ancestor gives file/path provenance, but cannot give a child's exact line.
                return new(source.File, source.Path + relativePath[ancestor.Length..], 0, 0);
            }
            int slash = ancestor.LastIndexOf('/');
            if (slash < 0) break;
            ancestor = ancestor[..slash];
        }
        return new(SourceFile, RootPath + relativePath, 0, 0);
    }
    internal string Fingerprint()
    {
        string json = JsonSerializer.Serialize(new
        {
            ScriptId, SourceFile, RootPath,
            Origins = SourceMap.OrderBy(p => p.Key, StringComparer.Ordinal).Select(p => new
            { Pointer = p.Key, p.Value.File, p.Value.Path, p.Value.Line, p.Value.ByteColumn }),
            Labels = Tags.OrderBy(p => p.Key, StringComparer.Ordinal).Select(p => new { p.Key, p.Value })
        });
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(json))).ToLowerInvariant();
    }
}
