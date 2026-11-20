using System.Text.Json;

namespace VMRuntime;

internal static class VmBuiltinNodes
{
    private static JsonElement Json(string text)
    { using var document = JsonDocument.Parse(text); return document.RootElement.Clone(); }
    private static VmNodeInputDefinition Input(string name, string description, string[]? types = null,
        bool required = true, bool variadic = false, bool nullAsEmpty = false)
        => new() { Name = name, Description = description, AcceptedTypes = types ?? ["any"],
            Required = required, Variadic = variadic, NullAsEmpty = nullAsEmpty };
    private static VmNodeParameterDefinition Parameter(string name, string description, string[]? types = null,
        bool required = true, JsonElement? defaultValue = null)
        => new() { Name = name, Description = description, AcceptedTypes = types ?? ["any"],
            Required = required, DefaultValue = defaultValue, MinLength = name is "name" or "function" or "method" ? 1 : 0 };
    private static VmNodeOutputDefinition Output(string[] types, string inference = "runtime", string description = "Result of the operation.")
        => new() { Types = types, Inference = inference, Description = description };
    internal static IReadOnlyList<VmLiteralTypeDefinition> LiteralTypes { get; } = Array.AsReadOnly(new[]
    {
        new VmLiteralTypeDefinition("vector2", "Two-component Float32 vector.", Parameter("x", "X component.", ["int","float"]), Parameter("y", "Y component.", ["int","float"])),
        new VmLiteralTypeDefinition("vector2i", "Two-component Int32 vector.", Parameter("x", "X component.", ["int"]), Parameter("y", "Y component.", ["int"])),
        new VmLiteralTypeDefinition("vector3", "Three-component Float32 vector.", Parameter("x", "X component.", ["int","float"]), Parameter("y", "Y component.", ["int","float"]), Parameter("z", "Z component.", ["int","float"])),
        new VmLiteralTypeDefinition("vector3i", "Three-component Int32 vector.", Parameter("x", "X component.", ["int"]), Parameter("y", "Y component.", ["int"]), Parameter("z", "Z component.", ["int"])),
        new VmLiteralTypeDefinition("color", "RGBA Float32 color; channels are not clamped.", Parameter("r", "Red channel.", ["int","float"]), Parameter("g", "Green channel.", ["int","float"]), Parameter("b", "Blue channel.", ["int","float"]), Parameter("a", "Alpha channel.", ["int","float"], false, Json("1.0")))
    });
    private static readonly string[] Numeric = ["int", "float", "vector2", "vector2i", "vector3", "vector3i", "color"];
    private static readonly string[] Ordered = ["int", "float", "bool"];

    internal static void Register(VmNodeRegistry registry)
    {
        void Simple(string id, string title, string category, string description, OpcodeId opcode,
            VmNodeInputDefinition[] inputs, VmNodeOutputDefinition output, string[]? aliases = null)
        {
            var example = new Dictionary<string, object?> { ["operation"] = id,
                ["inputs"] = inputs.ToDictionary(input => input.Name, Sample) };
            registry.Builtin(new()
            {
                Operation = id, Title = title, Category = category, Description = description,
                Inputs = inputs, Output = output, Aliases = aliases ?? [],
                Examples = [JsonSerializer.SerializeToElement(example)]
            }, VmNodeKind.Opcode, opcode);
        }
        VmNodeInputDefinition[] Pair(string[]? types = null) =>
            [Input("left", "Left operand, evaluated first.", types), Input("right", "Right operand, evaluated second.", types)];

        Simple("add", "Add", "math", "Adds compatible numbers/vectors/colors, or concatenates strings using VM conversion rules.", OpcodeId.Add, Pair(), Output([..Numeric, "string"], "vm_promotion"));
        Simple("subtract", "Subtract", "math", "Subtracts the right operand from the left using VM numeric promotion.", OpcodeId.Sub, Pair(), Output(Numeric, "vm_promotion"), ["sub"]);
        Simple("multiply", "Multiply", "math", "Multiplies numbers or corresponding vector/color components using VM numeric promotion.", OpcodeId.Mul, Pair(), Output(Numeric, "vm_promotion"), ["mul"]);
        Simple("divide", "Divide", "math", "Divides left by right. Integer zero divisors fail; floating-point division follows IEEE 754.", OpcodeId.Div, Pair(), Output(Numeric, "vm_promotion"), ["div"]);
        Simple("modulo", "Modulo", "math", "Computes integer remainder; promoted floating operands are truncated to Int32 by the existing conversion table.", OpcodeId.Mod, Pair(Ordered), Output(["int"]), ["mod"]);

        foreach (var entry in new[]
        {
            ("equal", "Equal", OpcodeId.Eq, "eq", "Tests equality after VM type promotion."),
            ("not_equal", "Not equal", OpcodeId.Neq, "neq", "Tests inequality after VM type promotion."),
            ("greater_than", "Greater than", OpcodeId.Gt, "gt", "Tests whether left is greater than right."),
            ("less_than", "Less than", OpcodeId.Lt, "lt", "Tests whether left is less than right."),
            ("greater_or_equal", "Greater or equal", OpcodeId.Ge, "ge", "Tests whether left is greater than or equal to right."),
            ("less_or_equal", "Less or equal", OpcodeId.Le, "le", "Tests whether left is less than or equal to right.")
        })
            Simple(entry.Item1, entry.Item2, "comparison", entry.Item5, entry.Item3,
                Pair(entry.Item1 is "equal" or "not_equal" ? null : Ordered), Output(["bool"]), [entry.Item4]);

        Simple("not", "Not", "logic", "Negates a value after its supported conversion to Bool.", OpcodeId.Not,
            [Input("value", "Value converted to Bool.", Ordered)], Output(["bool"]));
        Simple("negate", "Negate", "math", "Negates a number or each vector/color component.", OpcodeId.Neg,
            [Input("value", "Value to negate.", Numeric)], Output(Numeric, "same_as_input"), ["neg"]);
        Simple("min", "Minimum", "math", "Returns the original smaller operand after numeric comparison.", OpcodeId.Min, Pair(Ordered), Output(Ordered, "selected_input"));
        Simple("max", "Maximum", "math", "Returns the original larger operand after numeric comparison.", OpcodeId.Max, Pair(Ordered), Output(Ordered, "selected_input"));
        var range = new[] { Input("value", "Value to test or clamp.", Ordered), Input("min", "Inclusive lower bound.", Ordered), Input("max", "Inclusive upper bound.", Ordered) };
        Simple("clamp", "Clamp", "math", "Returns min, value or max according to numeric comparisons.", OpcodeId.Clamp, range, Output(Ordered, "selected_input"));
        Simple("in_range", "In range", "comparison", "Tests inclusive bounds: min <= value <= max.", OpcodeId.InRange, range, Output(["bool"]));

        var array = Input("array", "Mutable script array.", ["array"]);
        var index = Input("index", "Zero-based element index.", ["int"]);
        var value = Input("value", "Element value.");
        Simple("array_get", "Array get", "array", "Reads an array element by index.", OpcodeId.ArrayGet, [array, index], Output(["any"], "element_type"));
        Simple("array_set", "Array set", "array", "Replaces an array element and returns the same array.", OpcodeId.ArraySet, [array, index, value], Output(["array"]));
        Simple("array_remove_at", "Array remove", "array", "Removes an element and returns the same array.", OpcodeId.ArrayRemoveAt, [array, index], Output(["array"]));
        Simple("array_push", "Array push", "array", "Appends a value and returns the same array.", OpcodeId.ArrayPush, [array, value], Output(["array"]));
        Simple("array_length", "Array length", "array", "Returns the number of array elements.", OpcodeId.ArrayLength, [array], Output(["int"]));
        registry.BuiltinCompiler(new()
        {
            Operation = "array_contains", Title = "Array contains", Category = "array",
            Description = "Tests one value or an array of values for membership, using VM equality and numeric promotion.",
            Inputs = [array, Input("value", "Single value, or array of values to match.")],
            Parameters = [new() { Name = "mode", Description = "Match any or all values when value is an array.",
                AcceptedTypes = ["string"], Required = false, DefaultValue = Json("\"any\""), EnumValues = ["any", "all"] }],
            Output = Output(["bool"]), Examples = [Json("""{"operation":"array_contains","array":[1,2,3],"value":[1,3],"mode":"all"}""")]
        }, context =>
        {
            context.CompileAllInputs();
            context.Emit(OpcodeId.ArrayContains, a: context.Parameters["mode"].AsString() == "all" ? 1 : 0, b: 1);
        });
        Simple("array_concat", "Array concatenate", "array", "Creates a new array containing both arrays in order.", OpcodeId.ArrayConcat, Pair(["array"]), Output(["array"]));

        var target = Input("target", "Host object supplied by the game, normally through a variable.", ["object"]);
        var property = Input("property", "Property name.", ["string"]);
        var method = Input("method", "Exposed method name.", ["string"]);
        Simple("object_get", "Get property", "host", "Reads a property through the injected host adapter.", OpcodeId.ObjectGet, [target, property], Output(["any"]));
        Simple("object_set", "Set property", "host", "Writes a property and returns the target object.", OpcodeId.ObjectSet, [target, property, value], Output(["object"]));
        Simple("object_is_valid", "Object valid", "host", "Tests host object validity; Null returns false.", OpcodeId.ObjectIsValid,
            [Input("target", "Host object or Null.", ["object","null"])], Output(["bool"]));
        Simple("object_has_property", "Has property", "host", "Checks whether the host exposes a property.", OpcodeId.ObjectHasProperty, [target, property], Output(["bool"]));
        Simple("object_has_method", "Has method", "host", "Checks whether a method is permitted by the host adapter.", OpcodeId.ObjectHasMethod, [target, method], Output(["bool"]));
        Simple("object_get_property_list", "Property names", "host", "Returns property names reported by the host adapter.", OpcodeId.ObjectGetPropertyList, [target], Output(["array"]));
        Simple("object_get_method_list", "Method names", "host", "Returns permitted method names reported by the host adapter.", OpcodeId.ObjectGetMethodList, [target], Output(["array"]));

        void Special(string id, string title, string category, string description, VmNodeKind kind, string evaluation,
            VmNodeInputDefinition[] inputs, VmNodeParameterDefinition[] parameters, VmNodeOutputDefinition output, string example, string[]? aliases = null)
            => registry.Builtin(new()
            {
                Operation = id, Title = title, Category = category, Description = description, Evaluation = evaluation,
                Inputs = inputs, Parameters = parameters, Output = output, Examples = [Json(example)], Aliases = aliases ?? []
            }, kind);

        Special("constant", "Constant", "value", "Returns a literal; primitive JSON, arrays and tagged literals are shorthand forms.", VmNodeKind.Constant, "literal",
            [], [Parameter("value", "Literal value; objects become dictionaries unless tagged with $type.")], Output(["any"], "literal_type"),
            """{"operation":"constant","value":10}""", ["const"]);
        Special("read_variable", "Read variable", "value", "Reads a variable from this invocation; missing variables fail.", VmNodeKind.Variable, "variable",
            [], [Parameter("name", "Non-empty name of the invocation variable.", ["string"])], Output(["any"], "context_variable"),
            """{"operation":"read_variable","name":"hp_max_target"}""", ["variable", "load_variable"]);
        Special("if", "If", "control", "Evaluates the condition and only the selected branch. Missing else returns Null.", VmNodeKind.If, "conditional",
            [Input("condition", "Strict Bool selecting the branch.", ["bool"]), Input("then", "Expression evaluated when true."), Input("else", "Expression evaluated when false.", required: false)],
            [], Output(["any"], "selected_branch"), """{"operation":"if","inputs":{"condition":true,"then":1,"else":0}}""");
        Special("and", "And", "logic", "Evaluates right only when left is true; operands must be Bool.", VmNodeKind.And, "short_circuit",
            Pair(["bool"]), [], Output(["bool"]), """{"operation":"and","left":true,"right":false}""");
        Special("or", "Or", "logic", "Evaluates right only when left is false; operands must be Bool.", VmNodeKind.Or, "short_circuit",
            Pair(["bool"]), [], Output(["bool"]), """{"operation":"or","left":true,"right":false}""");
        Special("while", "While", "control", "Repeats body while condition is true; returns the last body result or Null. Execution limits still apply.", VmNodeKind.While, "loop",
            [Input("condition", "Bool expression re-evaluated before every iteration.", ["bool"]), Input("body", "Repeated expression.")],
            [], Output(["any"], "last_iteration"), """{"operation":"while","condition":false,"body":1}""");
        Special("array", "Create array", "array", "Evaluates items in order and creates a new array.", VmNodeKind.Array, "eager",
            [Input("items", "Ordered element expressions.", variadic: true)], [], Output(["array"]), """{"operation":"array","items":[1,2,3]}""");
        Special("sequence", "Sequence", "control", "Evaluates every step in order; returns the last result or Null for an empty list.", VmNodeKind.Sequence, "sequence",
            [Input("steps", "Ordered expressions.", variadic: true)], [], Output(["any"], "last_input"), """{"operation":"sequence","steps":[1,2]}""");
        Special("call", "Call function", "host", "Calls an explicitly registered host function once. The host defines its positional argument contract.", VmNodeKind.Call, "eager",
            [Input("args", "Positional argument expressions in source order; omitted/null means empty.", required: false, variadic: true, nullAsEmpty: true)],
            [Parameter("function", "Non-empty registered function name.", ["string"])], Output(["any"], "host_function"),
            """{"operation":"call","function":"example","args":[10]}""");
        Special("object_call", "Call method", "host", "Calls one permitted host method on the target object.", VmNodeKind.ObjectCall, "eager",
            [target, Input("args", "Positional argument expressions; omitted/null means empty.", required: false, variadic: true, nullAsEmpty: true)],
            [Parameter("method", "Non-empty method name; native Godot names use snake_case.", ["string"])], Output(["any"], "host_method"),
            """{"operation":"object_call","target":{"variable":"target"},"method":"get_name","args":[]}""");
    }

    private static object? Sample(VmNodeInputDefinition input)
    {
        if (input.Name == "target") return new Dictionary<string, object> { ["variable"] = "target" };
        if (input.AcceptedTypes.Contains("array")) return new[] { 1, 2 };
        if (input.AcceptedTypes.Contains("string")) return input.Name == "method" ? "get_name" : "name";
        if (input.AcceptedTypes.Count == 1 && input.AcceptedTypes.Contains("bool")) return true;
        return input.Name switch { "index" or "min" => 0, "max" => 10, "left" => 4, "right" => 2, _ => 1 };
    }
}
