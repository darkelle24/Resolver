using System.Text.Json;
using System.Text.Json.Serialization;

namespace VMRuntime.Diagnostics;

/// <summary>Base for data objects that can be printed as stable snake_case JSON.</summary>
public abstract class VmJsonObject
{
    /// <summary>Serializes this object, including specialized error properties.</summary>
    public string ToJson(bool indented = true)
        => JsonSerializer.Serialize(this, GetType(), VmJson.Options(indented));
}

internal static class VmJson
{
    private static readonly JsonSerializerOptions Compact = Create(false);
    private static readonly JsonSerializerOptions Pretty = Create(true);
    internal static JsonSerializerOptions Options(bool indented) => indented ? Pretty : Compact;
    private static JsonSerializerOptions Create(bool indented)
    {
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
            WriteIndented = indented,
            MaxDepth = 512,
            NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals
        };
        options.Converters.Add(new ErrorConverterFactory());
        options.Converters.Add(new TraceConverterFactory());
        return options;
    }
    private sealed class ErrorConverterFactory : JsonConverterFactory
    {
        public override bool CanConvert(Type type) => typeof(VmError).IsAssignableFrom(type);
        public override JsonConverter CreateConverter(Type type, JsonSerializerOptions options)
            => (JsonConverter)Activator.CreateInstance(typeof(ErrorConverter<>).MakeGenericType(type), true)!;
    }
    private sealed class ErrorConverter<TError> : JsonConverter<TError> where TError : VmError
    {
        public override TError? Read(ref Utf8JsonReader reader, Type type, JsonSerializerOptions options)
            => throw new NotSupportedException("Diagnostics are output-only.");
        public override void Write(Utf8JsonWriter writer, TError value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteString("kind", value.Kind);
            writer.WriteString("code", value.Code);
            writer.WriteString("message", value.Message);
            writer.WritePropertyName("source"); JsonSerializer.Serialize(writer, value.Source, options);
            if (value is VmExecutionError runtime)
            {
                writer.WritePropertyName("json_instruction"); JsonSerializer.Serialize(writer, runtime.JsonInstruction, options);
                writer.WritePropertyName("bytecode_instruction"); JsonSerializer.Serialize(writer, runtime.BytecodeInstruction, options);
            }
            switch (value)
            {
                case VmTypeMismatchError cast:
                    writer.WriteString("expected_type", cast.ExpectedType); writer.WriteString("actual_type", cast.ActualType); break;
                case VmVariableNotFoundError variable: writer.WriteString("variable_name", variable.VariableName); break;
                case VmHostCallError host: writer.WriteString("method", host.Method); break;
                case VmMissingFieldError missing: writer.WriteString("field", missing.Field); break;
            }
            writer.WriteEndObject();
        }
    }
    private sealed class TraceConverterFactory : JsonConverterFactory
    {
        public override bool CanConvert(Type type) => typeof(VmTraceInput).IsAssignableFrom(type);
        public override JsonConverter CreateConverter(Type type, JsonSerializerOptions options)
            => (JsonConverter)Activator.CreateInstance(typeof(TraceInputConverter<>).MakeGenericType(type), true)!;
    }
    private sealed class TraceInputConverter<TInput> : JsonConverter<TInput> where TInput : VmTraceInput
    {
        public override TInput? Read(ref Utf8JsonReader reader, Type type, JsonSerializerOptions options)
            => throw new NotSupportedException("Execution traces are output-only.");
        public override void Write(Utf8JsonWriter writer, TInput value, JsonSerializerOptions options)
        {
            switch (value)
            {
                case VmTraceLiteral literal: literal.Value.Value.WriteTo(writer); break;
                case VmTraceChild child: JsonSerializer.Serialize(writer, child.Node, options); break;
                case VmTraceOccurrences occurrences: JsonSerializer.Serialize(writer, occurrences.Nodes, options); break;
                default: throw new NotSupportedException(value.GetType().Name);
            }
        }
    }
}
