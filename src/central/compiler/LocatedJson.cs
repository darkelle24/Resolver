using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using VMRuntime.Diagnostics;

namespace VMRuntime;

internal sealed class LocatedJson
{
    internal string File { get; }
    internal JsonNode? Root { get; }
    private readonly Dictionary<string, VmSourceLocation> _locations = new(StringComparer.Ordinal);
    private readonly byte[] _bytes;
    private readonly List<int> _lines = new() { 0 };
    private string _currentPath = "";
    private readonly VmCompilationMetadata? _metadata;

    internal LocatedJson(string json, string file, VmCompilationMetadata? metadata = null)
    {
        File = file;
        _metadata = metadata;
        _bytes = Encoding.UTF8.GetBytes(json);
        if (_bytes.Length > 16 * 1024 * 1024)
            throw new VmCompileFault(new VmCompilationError("JSON_SIZE_LIMIT", "JSON exceeds 16 MiB.", metadata?.Resolve("") ?? new(file)));
        for (int i = 0; i < _bytes.Length; i++) if (_bytes[i] == 10) _lines.Add(i + 1);
        var reader = new Utf8JsonReader(_bytes, new JsonReaderOptions { MaxDepth = 128 });
        try
        {
            if (!reader.Read()) throw new JsonException("The JSON document is empty.");
            Root = ReadValue(ref reader, "");
            if (reader.Read()) throw new JsonException("Unexpected content after the root value.");
        }
        catch (JsonException error)
        {
            throw new VmCompileFault(new VmJsonSyntaxError(error.Message,
                metadata?.Resolve(_currentPath) ?? new(file, _currentPath, (error.LineNumber ?? 0) + 1, (error.BytePositionInLine ?? 0) + 1)));
        }
    }
    internal static string Child(string path, string name)
        => path + "/" + name.Replace("~", "~0", StringComparison.Ordinal).Replace("/", "~1", StringComparison.Ordinal);
    internal VmSourceLocation Locate(string path)
    {
        if (_metadata is not null) return _metadata.Resolve(path);
        string candidate = path;
        while (!_locations.TryGetValue(candidate, out var location))
        {
            int slash = candidate.LastIndexOf('/');
            if (slash < 0) return new(File, path);
            candidate = candidate[..slash];
        }
        var source = _locations[candidate];
        return new(File, path, source.Line, source.ByteColumn);
    }
    private VmSourceLocation At(string path, long offset)
    {
        if (_metadata is not null) return _metadata.Resolve(path);
        int line = _lines.BinarySearch((int)offset);
        if (line < 0) line = ~line - 1;
        return new(File, path, line + 1, offset - _lines[line] + 1);
    }
    internal string LogicalPath(string relativePath) => (_metadata?.RootPath ?? "") + relativePath;
    private JsonNode? ReadValue(ref Utf8JsonReader reader, string path)
    {
        _currentPath = path;
        _locations[path] = At(path, reader.TokenStartIndex);
        switch (reader.TokenType)
        {
            case JsonTokenType.StartObject:
            {
                var result = new JsonObject();
                while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
                {
                    if (reader.TokenType != JsonTokenType.PropertyName) throw new JsonException("Expected a property name.");
                    string name = reader.GetString()!;
                    string child = Child(path, name);
                    _currentPath = child;
                    if (result.ContainsKey(name))
                        throw new VmCompileFault(new VmCompilationError("JSON_DUPLICATE_KEY",
                            $"Duplicate property '{name}'.", At(child, reader.TokenStartIndex)));
                    if (!reader.Read()) throw new JsonException("Missing property value.");
                    result.Add(name, ReadValue(ref reader, child));
                }
                if (reader.TokenType != JsonTokenType.EndObject) throw new JsonException("Unterminated object.");
                return result;
            }
            case JsonTokenType.StartArray:
            {
                var result = new JsonArray();
                while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
                    result.Add(ReadValue(ref reader, Child(path, result.Count.ToString(System.Globalization.CultureInfo.InvariantCulture))));
                if (reader.TokenType != JsonTokenType.EndArray) throw new JsonException("Unterminated array.");
                return result;
            }
            case JsonTokenType.String: return JsonValue.Create(reader.GetString());
            case JsonTokenType.True: return JsonValue.Create(true);
            case JsonTokenType.False: return JsonValue.Create(false);
            case JsonTokenType.Null: return null;
            case JsonTokenType.Number:
            {
                var token = reader.ValueSpan;
                bool fractional = token.IndexOf((byte)'.') >= 0 || token.IndexOf((byte)'e') >= 0 || token.IndexOf((byte)'E') >= 0;
                if (!fractional && reader.TryGetInt32(out int integer)) return JsonValue.Create(integer);
                if (fractional && reader.TryGetSingle(out float number) && float.IsFinite(number)) return JsonValue.Create(number);
                throw new VmCompileFault(new VmCompilationError("JSON_NUMBER_RANGE",
                    fractional ? "Number is outside the finite Float32 range." : "Integer is outside the Int32 range.", Locate(path)));
            }
            default: throw new JsonException($"Unexpected token {reader.TokenType}.");
        }
    }
}
