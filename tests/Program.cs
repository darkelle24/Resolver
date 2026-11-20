using System.Text.Json;
using System.Text.Json.Nodes;
using VMRuntime;
using VMRuntime.Diagnostics;
using VMRuntime.Logging;
using ScriptProgram = VMRuntime.Program;

if (args.Length == 2 && args[0] == "--export-node-catalog")
{
    File.WriteAllText(args[1], new VmEngine().GetNodeCatalogJson(), new System.Text.UTF8Encoding(false));
    Console.WriteLine("Exported default node catalog: " + args[1]);
    return 0;
}

var tests = new List<(string Name, Action Run)>();
void Test(string name, Action run) => tests.Add((name, run));
void Check(bool condition, string message = "Assertion failed") { if (!condition) throw new Exception(message); }
CompiledScript Compile(VmEngine engine, string json, string file = "test.json")
{
    var compilation = engine.CompileJson(json, file);
    Check(compilation.Success, compilation.Error?.ToJson() ?? "Compilation failed.");
    return compilation.Script!;
}
VmTraceNode Child(VmTraceNode node, string key) => ((VmTraceChild)node.Inputs[key]).Node;
var explain = new VmExecutionOptions { Explain = true };

Test("compile once and inject different contexts", () =>
{
    var engine = new VmEngine();
    const string json = """{"operation":"multiply","left":{"variable":"population"},"right":{"variable":"tax_rate"}}""";
    var script = Compile(engine, json);
    Check(ReferenceEquals(script, Compile(engine, json)));
    var result = engine.Execute(script, new Dictionary<string, DynValue>
        { ["population"] = DynValue.FromInt(1000), ["tax_rate"] = DynValue.FromFloat(.15f) });
    Check(result.Success && Math.Abs(result.Value.AsFloat() - 150f) < .001f, result.Error?.ToJson() ?? "");
    Check(result.Explanation is null);
    var other = engine.Execute(script, new Dictionary<string, DynValue>
        { ["population"] = DynValue.FromInt(2000), ["tax_rate"] = DynValue.FromFloat(.15f) });
    Check(other.Value.AsFloat() == 300f);
});

Test("changed source gets a new handle; old handle still works", () =>
{
    var engine = new VmEngine();
    var first = Compile(engine, "1");
    var second = Compile(engine, "2");
    Check(!ReferenceEquals(first, second) && first.SourceHash != second.SourceHash);
    Check(engine.Execute(first).Value.AsInt() == 1 && engine.Execute(second).Value.AsInt() == 2);
    Check(first.NodeCatalog.RegistryVersion == first.RegistryVersion && first.NodeCatalog.RegistryVersion == second.NodeCatalog.RegistryVersion);
    using var savedCatalog = JsonDocument.Parse(first.GetNodeCatalogJson());
    Check(savedCatalog.RootElement.GetProperty("registry_version").GetInt64() == first.RegistryVersion);
});

Test("nested inputs have no redundant children field", () =>
{
    var engine = new VmEngine();
    var result = engine.Execute(Compile(engine, """{"operation":"multiply","inputs":{"left":5,"right":4}}"""), options: explain);
    var tree = result.Explanation!.Tree!;
    Check(tree.Output!.Value.GetInt32() == 20 && Child(tree, "left").Output!.Value.GetInt32() == 5);
    using var json = JsonDocument.Parse(result.Explanation.ToJson());
    Check(!json.RootElement.GetProperty("tree").TryGetProperty("children", out _));
    Check(json.RootElement.GetProperty("tree").GetProperty("inputs").GetProperty("left").GetProperty("operation").GetString() == "constant");
});

Test("if only executes and records selected branch", () =>
{
    var engine = new VmEngine();
    var script = Compile(engine, """{"operation":"if","inputs":{"condition":true,"then":7,"else":{"variable":"missing"}}}""");
    var result = engine.Execute(script, options: explain);
    Check(result.Success && result.Value.AsInt() == 7);
    var tree = result.Explanation!.Tree!;
    Check(tree.SelectedBranch == "then" && !tree.Inputs.ContainsKey("else"));
});

Test("false branch returns a genuine null when else is absent", () =>
{
    var engine = new VmEngine();
    var result = engine.Execute(Compile(engine, """{"operation":"if","condition":false,"then":7}"""), options: explain);
    Check(result.Success && result.Value.IsNull);
    Check(!result.Explanation!.Tree!.Inputs.ContainsKey("then"));
});

Test("selected branch is retained when its evaluation fails", () =>
{
    var engine = new VmEngine();
    var result = engine.Execute(Compile(engine,
        """{"operation":"if","condition":true,"then":{"variable":"missing"},"else":0}"""), options: explain);
    Check(result.Error is VmVariableNotFoundError && result.Explanation!.Tree!.SelectedBranch == "then");
});

Test("short circuit and/or skip missing variables", () =>
{
    var engine = new VmEngine();
    foreach (var (operation, left) in new[] { ("and", false), ("or", true) })
    {
        var source = """{"operation":"OP","left":LEFT,"right":{"variable":"missing"}}"""
            .Replace("OP", operation).Replace("LEFT", left ? "true" : "false");
        var result = engine.Execute(Compile(engine, source), options: explain);
        Check(result.Success && result.Value.AsBool() == left);
        Check(!result.Explanation!.Tree!.Inputs.ContainsKey("right"));
    }
});

Test("missing variable has local diagnostics with explain disabled", () =>
{
    var engine = new VmEngine();
    var script = Compile(engine, """{"operation":"multiply","left":{"variable":"missing"},"right":2}""", "mods/tax.json");
    var result = engine.Execute(script);
    Check(result.Error is VmVariableNotFoundError { VariableName: "missing" });
    Check(result.Explanation is null && result.Error!.Source.File == "mods/tax.json" && result.Error.Source.Path == "/left");
    Check(result.Error!.BytecodeInstruction.Opcode == "LoadVariable");
    Check(result.Error.BytecodeInstruction.Inputs["value"].Type == "unavailable");
    Check(script.GetInstructionSource(result.Error.BytecodeInstruction.InstructionIndex).Path == "/left");
});

Test("error is attached to failing tree node and shared with result", () =>
{
    var engine = new VmEngine();
    var result = engine.Execute(Compile(engine, """{"operation":"add","left":5,"right":{"variable":"missing"}}"""), options: explain);
    var tree = result.Explanation!.Tree!;
    Check(ReferenceEquals(Child(tree, "right").Error, result.Error));
    Check(Child(tree, "left").Output!.Value.GetInt32() == 5);
    Check(tree.Output is null && tree.Status == "aborted");
    using var json = JsonDocument.Parse(result.ToJson());
    Check(json.RootElement.GetProperty("error").GetProperty("variable_name").GetString() == "missing");
    Check(json.RootElement.GetProperty("explanation").GetProperty("tree").GetProperty("inputs")
        .GetProperty("right").GetProperty("error").GetProperty("code").GetString() == "VARIABLE_NOT_FOUND");
});

Test("division zero keeps operands after opcode popped them", () =>
{
    var engine = new VmEngine();
    var result = engine.Execute(Compile(engine, """{"operation":"divide","left":1000,"right":0}"""));
    Check(result.Error is VmDivisionByZeroError);
    Check(result.Error!.JsonInstruction.Inputs["left"].Value.GetInt32() == 1000);
    Check(result.Error.BytecodeInstruction.Inputs["right"].Value.GetInt32() == 0);
    Check(result.Error.BytecodeInstruction.Opcode == "Div");
});

Test("typed accessor failures preserve expected/actual types", () =>
{
    var engine = new VmEngine();
    var result = engine.Execute(Compile(engine, """{"operation":"not","value":"text"}"""));
    Check(result.Error is VmTypeMismatchError { ExpectedType: "Bool", ActualType: "String" });
    using var json = JsonDocument.Parse(result.Error!.ToJson());
    Check(json.RootElement.GetProperty("expected_type").GetString() == "Bool");
});

Test("syntax errors identify file and line", () =>
{
    var result = new VmEngine().CompileJson("{\n\"operation\": }\n", "broken.json");
    Check(result.Error is VmJsonSyntaxError && result.Error.Source.File == "broken.json" && result.Error.Source.Line == 2);
});

Test("duplicate properties and missing inputs are rejected", () =>
{
    var engine = new VmEngine();
    Check(engine.CompileJson("""{"operation":"add","left":1,"left":2,"right":3}""").Error!.Code == "JSON_DUPLICATE_KEY");
    Check(engine.CompileJson("""{"operation":"add","left":1}""").Error is VmMissingFieldError { Field: "right" });
    Check(engine.CompileJson("""{"operation":"add","left":1,"inputs":{"left":2,"right":3}}""").Error!.Code == "JSON_AMBIGUOUS_INPUT");
});

Test("numeric range policy rejects oversized integers and nonfinite floats", () =>
{
    var engine = new VmEngine();
    Check(engine.CompileJson("2147483648").Error!.Code == "JSON_NUMBER_RANGE");
    Check(engine.CompileJson("1e100").Error!.Code == "JSON_NUMBER_RANGE");
    Check(engine.Execute(Compile(engine, "1.0")).Value.Type == VMRuntime.ValueType.Float);
});

Test("UTF8 byte columns and escaped JSON pointers", () =>
{
    var engine = new VmEngine();
    const string source = """{"operation":"constant","value":{"é/a~b":1}}""";
    var script = Compile(engine, source);
    Check(script.OriginalJson == source);
    const string unicodeSource = """{"operation":"add","left":"é","right":{"variable":"x"}}""";
    var result = engine.CompileJson(unicodeSource);
    Check(result.Success);
    int ip = Enumerable.Range(0, result.Script!.InstructionCount)
        .First(i => result.Script.GetInstructionSource(i).Path == "/right");
    int expectedColumn = System.Text.Encoding.UTF8.GetByteCount(unicodeSource[..unicodeSource.IndexOf("{\"variable\"", StringComparison.Ordinal)]) + 1;
    Check(result.Script.GetInstructionSource(ip).ByteColumn == expectedColumn);
});

Test("vectors and colors operate without Godot", () =>
{
    var engine = new VmEngine();
    var result = engine.Execute(Compile(engine,
        """{"operation":"add","left":{"$type":"vector2","x":1,"y":2},"right":{"$type":"vector2","x":3,"y":4}}"""));
    Check(result.Success && result.Value.AsVector2() == new ScriptVector2(4, 6));
    Check(DynValue.FromColor(new ScriptColor(1, 2, 3)).AsColor().A == 1);
});

Test("constant collections are not shared between invocations", () =>
{
    var engine = new VmEngine();
    var script = Compile(engine, "[1,2]");
    var first = engine.Execute(script).Value.AsArray();
    first.Add(DynValue.FromInt(9));
    Check(engine.Execute(script).Value.AsArray().Count == 2);
});

Test("dictionary keys use coherent equality", () =>
{
    var array = DynValue.FromArray(new());
    var map = new Dictionary<DynValue, DynValue> { [array] = DynValue.FromInt(5) };
    array.AsArray().Add(DynValue.FromInt(9));
    Check(map[array].AsInt() == 5 && array.Equals(array));
    var nan = DynValue.FromFloat(float.NaN);
    Check(nan.Equals(nan) && DynValue.FromFloat(-0f).GetHashCode() == DynValue.FromFloat(0f).GetHashCode());
    Check(default(DynValue).IsNull);
});

Test("host callbacks receive arguments in source order and run once", () =>
{
    var engine = new VmEngine();
    int calls = 0;
    engine.RegisterFunction("subtract", args => { calls++; return DynValue.FromInt(args[0].AsInt() - args[1].AsInt()); });
    var result = engine.Execute(Compile(engine, """{"operation":"call","function":"subtract","args":[7,2]}"""), options: explain);
    Check(result.Success && result.Value.AsInt() == 5 && calls == 1);
});

Test("host failures become typed diagnostics", () =>
{
    var engine = new VmEngine();
    engine.RegisterFunction("broken", _ => throw new Exception("host failure"));
    var result = engine.Execute(Compile(engine, """{"operation":"call","function":"broken"}"""));
    Check(result.Error is VmHostCallError { Method: "broken" });
});

Test("host objects are injected through a pure C# adapter", () =>
{
    var engine = new VmEngine(host: new FakeHost());
    var result = engine.Execute(Compile(engine,
        """{"operation":"object_call","target":{"variable":"obj"},"method":"subtract","args":[7,2]}"""),
        new Dictionary<string, DynValue> { ["obj"] = DynValue.FromObject(new object()) });
    Check(result.Success && result.Value.AsInt() == 5, result.Error?.ToJson() ?? "");
});

Test("repeated loop evaluations preserve execution occurrences", () =>
{
    var engine = new VmEngine();
    int count = 0;
    engine.RegisterFunction("condition", _ => DynValue.FromBool(count < 3));
    engine.RegisterFunction("body", _ => DynValue.FromInt(++count));
    var result = engine.Execute(Compile(engine,
        """{"operation":"while","condition":{"operation":"call","function":"condition"},"body":{"operation":"call","function":"body"}}"""),
        options: explain);
    Check(result.Success && result.Value.AsInt() == 3);
    var tree = result.Explanation!.Tree!;
    Check(((VmTraceOccurrences)tree.Inputs["condition"]).Nodes.Count == 4);
    Check(((VmTraceOccurrences)tree.Inputs["body"]).Nodes.Count == 3);
});

Test("trace truncation does not change execution", () =>
{
    var engine = new VmEngine();
    var result = engine.Execute(Compile(engine, """{"operation":"multiply","left":7,"right":6}"""),
        options: new() { Explain = true, MaxTraceSteps = 1 });
    Check(result.Success && result.Value.AsInt() == 42 && result.Explanation!.Truncated);
});

Test("failure path is preserved even after trace budget is exhausted", () =>
{
    var engine = new VmEngine();
    var result = engine.Execute(Compile(engine, """{"operation":"add","left":1,"right":{"variable":"missing"}}"""),
        options: new() { Explain = true, MaxTraceSteps = 0 });
    Check(result.Explanation!.Truncated);
    Check(ReferenceEquals(Child(result.Explanation.Tree!, "right").Error, result.Error));
});

Test("instruction budget terminates infinite loops with source context", () =>
{
    var engine = new VmEngine();
    var result = engine.Execute(Compile(engine, """{"operation":"while","condition":true,"body":1}"""),
        options: new() { Explain = true, MaxInstructions = 80, MaxTraceSteps = 5 });
    Check(result.Error?.Code == "EXECUTION_LIMIT" && result.Explanation!.Truncated);
});

Test("trace values survive later collection mutation", () =>
{
    var engine = new VmEngine();
    var array = new List<DynValue> { DynValue.FromInt(1) };
    engine.RegisterFunction("mutate", _ => { array.Add(DynValue.FromInt(2)); return DynValue.Null; });
    var result = engine.Execute(Compile(engine,
        """{"operation":"sequence","steps":[{"variable":"array"},{"operation":"call","function":"mutate"}]}"""),
        new Dictionary<string, DynValue> { ["array"] = DynValue.FromArray(array) }, explain);
    Check(Child(result.Explanation!.Tree!, "steps/0").Output!.Value.GetArrayLength() == 1);
    Check(array.Count == 2);
});

Test("snapshots handle reference cycles and bound large inputs", () =>
{
    var list = new List<DynValue>();
    var value = DynValue.FromArray(list);
    list.Add(value);
    var snapshot = VmValueSnapshot.Capture(value);
    Check(snapshot.Truncated && snapshot.Value[0].GetString() == "<cycle>");
});

Test("one diagnostic is logged per failure", () =>
{
    var logger = new CollectingLogger();
    var engine = new VmEngine(logger);
    engine.CompileJson("{");
    engine.Execute(Compile(engine, """{"variable":"missing"}"""));
    Check(logger.Errors.Count == 2);
});

Test("export includes every instruction binding and original source", () =>
{
    var engine = new VmEngine();
    var script = Compile(engine, """{"operation":"if","condition":true,"then":1,"else":2}""");
    using var json = JsonDocument.Parse(script.ExportProgramJson());
    Check(json.RootElement.GetProperty("instructions").GetArrayLength() == script.InstructionCount);
    foreach (var instruction in json.RootElement.GetProperty("instructions").EnumerateArray())
        Check(instruction.GetProperty("node_id").GetInt32() >= 0);
    Check(script.Nodes.All(n => n.Source.File == "test.json"));
    Check(script.Nodes[0].ParentNodeId is null);
    Check(script.Nodes.Skip(1).All(n => n.ParentNodeId is int parent && parent < n.Id && n.ParentInput is not null));
});

Test("instruction limits identify the failed source node at every boundary", () =>
{
    var engine = new VmEngine();
    var script = Compile(engine, """{"operation":"multiply","left":7,"right":6}""");
    IEnumerable<VmTraceNode> Visit(VmTraceNode node)
    {
        yield return node;
        foreach (var input in node.Inputs.Values)
            if (input is VmTraceChild child)
                foreach (var descendant in Visit(child.Node)) yield return descendant;
    }
    for (int limit = 1; limit < script.InstructionCount; limit++)
    {
        var result = engine.Execute(script, options: new() { Explain = true, MaxInstructions = limit, MaxTraceSteps = 0 });
        Check(result.Error?.Code == "EXECUTION_LIMIT");
        var failed = Visit(result.Explanation!.Tree!).Single(n => n.Error is not null);
        Check(failed.Path == result.Error!.Source.Path);
    }
});

Test("explicit null is a value while a null input container is invalid", () =>
{
    var engine = new VmEngine();
    Check(engine.Execute(Compile(engine, """{"operation":"constant","value":null}""")).Value.IsNull);
    Check(engine.CompileJson("""{"operation":"add","inputs":null,"left":1,"right":2}""").Error!.Code == "JSON_TYPE");
});

Test("legacy assembler append relocates control flow", () =>
{
    var engine = new VmEngine();
    var program = new ScriptProgram();
    var assembler = new Assembler(program);
    assembler.Append(Compile(engine, "5"));
    assembler.Append(Compile(engine, """{"operation":"if","condition":false,"then":1,"else":2}"""));
    var vm = new VMRuntime.VM();
    foreach (var instruction in program.OpcodeList) { }
    // ProcessJson/Append can also be used with a VM's own mutable program.
    var destination = new Assembler(vm.Program);
    destination.Append(Compile(engine, "5"));
    destination.Append(Compile(engine, """{"operation":"if","condition":false,"then":1,"else":2}"""));
    vm.Run();
    Check(vm.Stack.Pull().AsInt() == 2 && vm.Stack.Pull().AsInt() == 5);
});

Test("the core assembly has no Godot assembly references", () =>
{
    Check(typeof(VmEngine).Assembly.GetReferencedAssemblies().All(a => !a.Name!.Contains("Godot")));
    Check(System.Runtime.InteropServices.Marshal.SizeOf(typeof(DynValue).Assembly.GetType("VMRuntime.ValueUnion")!) == 16);
});

Test("registered legacy RuleNode compilers remain usable", () =>
{
    global::VM.RuleNodeSystem.RuleNodeFactory.Register("LegacySmoke", () => new LegacySmokeNode());
    var vm = new VMRuntime.VM();
    global::VM.RuleNodeSystem.RuleNodeProcessor.ProcessJson("""{"type":"LegacySmoke"}""", new Assembler(vm.Program));
    vm.Run();
    Check(vm.Stack.Pull().AsInt() == 13);
});

Test("compile only onAttack from a merged item", () =>
{
    var engine = new VmEngine();
    var item = JsonNode.Parse("""{"name":"Excalibur","price":"10","onEquip":{"operation":"unknown"},"onAttack":{"operation":"multiply","inputs":{"left":{"variable":"hp_max_target"},"right":10}}}""")!;
    string original = item.ToJsonString();
    var metadata = new VmCompilationMetadata { ScriptId = "items/sword:onAttack", SourceFile = "items.json", RootPath = "/onAttack" };
    var compilation = engine.CompileFragment(item["onAttack"], metadata);
    Check(compilation.Success, compilation.Error?.ToJson() ?? "");
    var result = engine.Execute(compilation.Script!, new Dictionary<string, DynValue> { ["hp_max_target"] = DynValue.FromInt(50) }, explain);
    Check(result.Success && result.Value.AsInt() == 500);
    Check(item.ToJsonString() == original, "Compilation mutated the merged item.");
    Check(compilation.Script!.OriginalJson.Contains("hp_max_target") && !compilation.Script.OriginalJson.Contains("Excalibur"));
    Check(result.Explanation!.RootPath == "/onAttack" && result.Explanation.IsFragment);
    Check(result.Explanation.Tree!.Path == "/onAttack" && Child(result.Explanation.Tree, "left").Path == "/onAttack/inputs/left");
});

Test("non-script item changes do not invalidate the selected fragment", () =>
{
    var engine = new VmEngine();
    var item = JsonNode.Parse("""{"name":"Excalibur","price":10,"onAttack":{"operation":"multiply","left":5,"right":10}}""")!;
    var metadata = new VmCompilationMetadata { SourceFile = "sword.json", RootPath = "/onAttack" };
    var first = engine.CompileFragment(item["onAttack"], metadata).Script!;
    item["name"] = "Renamed"; item["price"] = 999;
    Check(ReferenceEquals(first, engine.CompileFragment(item["onAttack"], metadata).Script));
    item["onAttack"]!["right"] = 20;
    var changed = engine.CompileFragment(item["onAttack"], metadata).Script!;
    Check(!ReferenceEquals(first, changed));
    Check(engine.Execute(first).Value.AsInt() == 50 && engine.Execute(changed).Value.AsInt() == 100);
});

Test("identical fragments at different item events keep independent paths", () =>
{
    var engine = new VmEngine();
    var fragment = JsonNode.Parse("""{"variable":"missing"}""");
    var attack = engine.CompileFragment(fragment, new() { SourceFile = "sword.json", RootPath = "/onAttack" }).Script!;
    var equip = engine.CompileFragment(fragment, new() { SourceFile = "sword.json", RootPath = "/onEquip" }).Script!;
    Check(!ReferenceEquals(attack, equip) && attack.SourceHash == equip.SourceHash);
    Check(engine.Execute(attack).Error!.Source.Path == "/onAttack");
    Check(engine.Execute(equip).Error!.Source.Path == "/onEquip");
});

Test("merged paths and original mod locations remain distinct", () =>
{
    var engine = new VmEngine();
    var fragment = JsonNode.Parse("""{"operation":"multiply","inputs":{"left":{"variable":"hp_max_target"},"right":10}}""");
    var metadata = new VmCompilationMetadata
    {
        ScriptId = "items/sword:onAttack", RootPath = "/items/sword/onAttack",
        SourceMap = new Dictionary<string, VmSourceLocation>
        {
            ["/inputs/left"] = new("mods/base/sword.json", "/effects/hp", 40, 9),
            ["/inputs/right"] = new("mods/balance/patch.json", "/damage", 7, 3)
        },
        Tags = new Dictionary<string, string> { ["item_id"] = "sword" }
    };
    var compiled = engine.CompileFragment(fragment, metadata);
    Check(compiled.Success, compiled.Error?.ToJson() ?? "");
    var plain = engine.Execute(compiled.Script!);
    Check(plain.Explanation is null && plain.Error is VmVariableNotFoundError);
    Check(plain.Error!.Source.File == "mods/base/sword.json" && plain.Error.Source.Path == "/effects/hp" && plain.Error.Source.Line == 40);
    Check(plain.Error.JsonInstruction.Path == "/items/sword/onAttack/inputs/left");
    Check(compiled.Script!.GetInstructionSource(plain.Error.BytecodeInstruction.InstructionIndex).File == "mods/base/sword.json");
    var traced = engine.Execute(compiled.Script, options: explain);
    var left = Child(traced.Explanation!.Tree!, "left");
    Check(left.Path == "/items/sword/onAttack/inputs/left" && left.Source.Path == "/effects/hp");
    Check(ReferenceEquals(left.Error, traced.Error));
    Check(compiled.Script.Nodes.Single(node => node.Path.EndsWith("/inputs/right")).Source.File == "mods/balance/patch.json");
});

Test("fragment cache refreshes changed provenance and freezes supplied metadata", () =>
{
    var engine = new VmEngine();
    var origins = new Dictionary<string, VmSourceLocation> { [""] = new("base.json", "/onAttack", 10, 2) };
    var tags = new Dictionary<string, string> { ["item_id"] = "sword" };
    var metadata = new VmCompilationMetadata { RootPath = "/onAttack", SourceMap = origins, Tags = tags };
    var fragment = JsonNode.Parse("""{"variable":"missing"}""");
    var first = engine.CompileFragment(fragment, metadata).Script!;
    origins[""] = new("patch.json", "/attack", 30, 4);
    tags["item_id"] = "axe";
    var second = engine.CompileFragment(fragment, metadata).Script!;
    Check(first.SourceHash == second.SourceHash && first.MetadataHash != second.MetadataHash && !ReferenceEquals(first, second));
    Check(engine.Execute(first).Error!.Source.File == "base.json" && first.Metadata.Tags["item_id"] == "sword");
    Check(engine.Execute(second).Error!.Source.File == "patch.json" && second.Metadata.Tags["item_id"] == "axe");
});

Test("ancestor provenance does not fabricate child line numbers", () =>
{
    var engine = new VmEngine();
    var metadata = new VmCompilationMetadata
    {
        RootPath = "/onAttack",
        SourceMap = new Dictionary<string, VmSourceLocation> { [""] = new("base.json", "/sword/attack", 12, 5) }
    };
    var script = engine.CompileFragment(JsonNode.Parse("""{"operation":"add","left":{"variable":"missing"},"right":1}"""), metadata).Script!;
    Check(script.Nodes[0].Source.HasPosition);
    var error = engine.Execute(script).Error!;
    Check(error.Source.File == "base.json" && error.Source.Path == "/sword/attack/left");
    Check(!error.Source.HasPosition && error.Source.Line == 0 && error.Source.ByteColumn == 0);
});

Test("fragment metadata is validated with typed compilation diagnostics", () =>
{
    var engine = new VmEngine();
    Check(engine.CompileFragment(JsonValue.Create(1), new() { RootPath = "onAttack" }).Error!.Code == "INVALID_METADATA");
    Check(engine.CompileFragment(JsonValue.Create(1), new() { RootPath = "/bad~escape" }).Error!.Code == "INVALID_METADATA");
    Check(engine.CompileFragment(JsonValue.Create(1), new()
    {
        SourceMap = new Dictionary<string, VmSourceLocation> { [""] = new("file", "", 10, 0) }
    }).Error!.Code == "INVALID_METADATA");
});

Test("JsonElement fragments survive disposal of their containing document", () =>
{
    var engine = new VmEngine();
    CompiledScript script;
    using (var document = JsonDocument.Parse("""{"name":"sword","onAttack":{"operation":"multiply","left":8,"right":10}}"""))
        script = engine.CompileFragment(document.RootElement.GetProperty("onAttack"), new() { RootPath = "/onAttack" }).Script!;
    Check(engine.Execute(script).Value.AsInt() == 80);
});

Test("parsed floating-point constants retain their VM type", () =>
{
    var engine = new VmEngine();
    var compiled = engine.CompileFragment(JsonValue.Create(10.0), new() { RootPath = "/onAttack" });
    Check(compiled.Success, compiled.Error?.ToJson() ?? "");
    Check(engine.Execute(compiled.Script!).Value.Type == VMRuntime.ValueType.Float);
    Check(engine.CompileFragment(JsonValue.Create(double.PositiveInfinity), new()).Error is not null);
});

Test("fragment syntax errors use supplied provenance instead of serialized line numbers", () =>
{
    var result = new VmEngine().CompileFragmentJson("{\n\"operation\": }", new() { SourceFile = "merged.json", RootPath = "/onAttack" });
    Check(result.Error is VmJsonSyntaxError && !result.Error.Source.HasPosition);
    Check(result.Error!.Source.Path == "/onAttack/operation");
});

Test("metadata map ordering does not invalidate cached fragments", () =>
{
    var engine = new VmEngine();
    var first = new VmCompilationMetadata
    {
        RootPath = "/onAttack", Tags = new Dictionary<string, string> { ["a"] = "1", ["b"] = "2" },
        SourceMap = new Dictionary<string, VmSourceLocation> { [""] = new("root"), ["/left"] = new("child") }
    };
    var second = new VmCompilationMetadata
    {
        RootPath = "/onAttack", Tags = new Dictionary<string, string> { ["b"] = "2", ["a"] = "1" },
        SourceMap = new Dictionary<string, VmSourceLocation> { ["/left"] = new("child"), [""] = new("root") }
    };
    var node = JsonValue.Create(1);
    Check(ReferenceEquals(engine.CompileFragment(node, first).Script, engine.CompileFragment(node, second).Script));
});

Test("catalog describes every built-in and all examples compile", () =>
{
    var engine = new VmEngine();
    var catalog = engine.GetNodeCatalog();
    Check(catalog.Nodes.Count == 41 && catalog.LiteralTypes.Count == 5 && catalog.ValueTypes.Count == 13);
    foreach (var node in catalog.Nodes)
    {
        Check(!string.IsNullOrWhiteSpace(node.Description) && node.Examples.Count > 0 && node.Origin == "builtin");
        Check(node.Inputs.All(input => input.Description.Length > 0));
        foreach (var example in node.Examples)
        {
            var compilation = engine.CompileJson(example.GetRawText(), "catalog:" + node.Operation);
            Check(compilation.Success, node.Operation + ": " + compilation.Error?.ToJson());
            if (node.Category != "host" && node.Operation != "read_variable")
            {
                var execution = engine.Execute(compilation.Script!);
                Check(execution.Success, "Example execution " + node.Operation + ": " + execution.Error?.ToJson());
            }
        }
    }
    using var json = JsonDocument.Parse(engine.GetNodeCatalogJson());
    Check(json.RootElement.GetProperty("format_version").GetInt32() == 1);
    Check(json.RootElement.GetProperty("registry_hash").GetString() == catalog.RegistryHash);
    Check(json.RootElement.GetProperty("nodes").GetArrayLength() == 41);
});

Test("array membership documents and executes scalar, any and all modes", () =>
{
    var engine = new VmEngine();
    Check(engine.Execute(Compile(engine, """{"operation":"array_contains","array":[1,2,3],"value":2}""")).Value.AsBool());
    Check(engine.Execute(Compile(engine, """{"operation":"array_contains","array":[1,2,3],"value":[7,2]}""")).Value.AsBool());
    Check(!engine.Execute(Compile(engine, """{"operation":"array_contains","array":[1,2,3],"value":[7,2],"mode":"all"}""")).Value.AsBool());
    Check(engine.CompileJson("""{"operation":"array_contains","array":[1],"value":1,"mode":"bad"}""").Error!.Code == "JSON_ENUM");
});

Test("a node class supplies implementation and exported documentation", () =>
{
    var engine = new VmEngine();
    string before = engine.GetNodeCatalog().RegistryHash;
    engine.RegisterNode(new AddPercentNode());
    var definition = engine.GetNodeCatalog().Nodes.Single(node => node.Operation == "add_percent");
    Check(definition.Origin == "project" && definition.Output.Types.Single() == "float");
    Check(before != engine.GetNodeCatalog().RegistryHash && engine.GetNodeCatalog().Nodes.Count == 42);
    var script = Compile(engine, """{"operation":"boost","value":100,"percent":25}""");
    var result = engine.Execute(script, options: explain);
    Check(result.Success && result.Value.AsFloat() == 125);
    Check(result.Explanation!.Tree!.Operation == "add_percent");
    Check(((VmTraceLiteral)result.Explanation.Tree.Inputs["rounding"]).Value.Value.GetString() == "none");
    Check(script.RegistryHash == engine.GetNodeCatalog().RegistryHash && result.Explanation.RegistryVersion == script.RegistryVersion);
});

Test("registered parameters validate type, enum and requiredness", () =>
{
    var engine = new VmEngine();
    engine.RegisterNode(new AddPercentNode());
    Check(engine.CompileJson("""{"operation":"add_percent","value":100,"percent":25,"rounding":"bad"}""").Error!.Code == "JSON_ENUM");
    Check(engine.CompileJson("""{"operation":"add_percent","value":100,"percent":25,"rounding":7}""").Error!.Code == "JSON_TYPE");
    Check(engine.CompileJson("""{"operation":"add_percent","value":100}""").Error is VmMissingFieldError { Field: "percent" });
});

Test("custom input type errors retain JSON and bytecode inputs", () =>
{
    var engine = new VmEngine(); engine.RegisterNode(new AddPercentNode());
    var script = Compile(engine, """{"operation":"add_percent","value":{"variable":"amount"},"percent":25}""", "custom.json");
    var result = engine.Execute(script, new Dictionary<string, DynValue> { ["amount"] = DynValue.FromString("bad") }, explain);
    Check(result.Error is VmTypeMismatchError && result.Error.BytecodeInstruction.Opcode == "InvokeNode");
    Check(result.Error!.Source.File == "custom.json" && result.Error.JsonInstruction.Inputs["value"].Value.GetString() == "bad");
    Check(result.Error.BytecodeInstruction.Inputs["value"].Value.GetString() == "bad");
    Check(ReferenceEquals(result.Explanation!.Tree!.Error, result.Error));
});

Test("custom node output contracts are enforced", () =>
{
    var engine = new VmEngine();
    engine.RegisterNode(new() { Operation = "bad_output", Description = "Deliberately invalid implementation.", Output = new() { Types = ["int"] } }, _ => DynValue.FromString("wrong"));
    var result = engine.Execute(Compile(engine, """{"operation":"bad_output"}"""));
    Check(result.Error is VmTypeMismatchError { ExpectedType: "output:int" });
});

Test("variadic node inputs validate cardinality and preserve order", () =>
{
    var engine = new VmEngine();
    engine.RegisterNode(new()
    {
        Operation = "weighted_sum", Description = "Weights ordered values by their one-based position.",
        Inputs = [new() { Name = "values", Description = "Values in order.", AcceptedTypes = ["int"], Variadic = true, MinItems = 1 }],
        Output = new() { Types = ["int"] }
    }, args => DynValue.FromInt(args.GetInputs("values").Select((value, i) => value.AsInt() * (i + 1)).Sum()));
    Check(engine.Execute(Compile(engine, """{"operation":"weighted_sum","values":[1,2,3]}""")).Value.AsInt() == 14);
    Check(engine.CompileJson("""{"operation":"weighted_sum","values":[]}""").Error!.Code == "JSON_INPUT_COUNT");
    Check(engine.CompileJson("""{"operation":"weighted_sum","values":1}""").Error!.Code == "JSON_TYPE");
});

Test("replacing a project node invalidates cache and preserves old handles", () =>
{
    var engine = new VmEngine();
    var definition = new VmNodeDefinition { Operation = "project_value", Description = "Returns the current registered implementation's value.", Output = new() { Types = ["int"] } };
    engine.RegisterNode(definition, _ => DynValue.FromInt(1));
    const string source = """{"operation":"project_value"}""";
    var first = Compile(engine, source);
    engine.RegisterNode(definition, _ => DynValue.FromInt(2), replace: true);
    var second = Compile(engine, source);
    Check(!ReferenceEquals(first, second) && first.SourceHash == second.SourceHash && first.RegistryVersion != second.RegistryVersion);
    Check(first.NodeCatalog.RegistryVersion == first.RegistryVersion && first.NodeCatalog.RegistryVersion != second.NodeCatalog.RegistryVersion);
    Check(engine.Execute(first).Value.AsInt() == 1 && engine.Execute(second).Value.AsInt() == 2);
    Check(ReferenceEquals(second, Compile(engine, source)));
});

Test("registry rejects collisions and malformed definitions atomically", () =>
{
    var registry = new VmNodeRegistry();
    long revision = registry.GetCatalog().RegistryVersion;
    void Reject(VmNodeDefinition definition, bool replace = false)
    {
        bool rejected = false;
        try { registry.Register(definition, _ => DynValue.Null, replace); } catch (ArgumentException) { rejected = true; }
        Check(rejected);
    }
    Reject(new() { Operation = "multiply", Description = "Cannot override built-ins." }, true);
    Reject(new() { Operation = "mul", Description = "Cannot claim a built-in alias." });
    Reject(new() { Operation = "custom", Description = "Alias collision.", Aliases = ["mul"] });
    Reject(new() { Operation = "custom", Description = "Duplicate names.", Inputs = [new() { Name = "value", Description = "Input." }], Parameters = [new() { Name = "value", Description = "Parameter." }] });
    Reject(new() { Operation = "custom", Description = "Unknown type.", Inputs = [new() { Name = "value", Description = "Input.", AcceptedTypes = ["nonexistent"] }] });
    Reject(new() { Operation = "custom" });
    Check(registry.GetCatalog().RegistryVersion == revision && registry.GetCatalog().Nodes.Count == 41);
});

Test("registered definitions snapshot mutable caller lists", () =>
{
    var engine = new VmEngine();
    var inputs = new List<VmNodeInputDefinition> { new() { Name = "value", Description = "Original input." } };
    var aliases = new List<string> { "original_alias" };
    engine.RegisterNode(new() { Operation = "snapshot_node", Description = "Returns its input.", Inputs = inputs, Aliases = aliases }, args => args.GetInput("value"));
    inputs.Clear(); aliases.Clear();
    var definition = engine.GetNodeCatalog().Nodes.Single(node => node.Operation == "snapshot_node");
    Check(definition.Inputs.Count == 1 && definition.Aliases.Single() == "original_alias");
    Check(engine.Execute(Compile(engine, """{"operation":"original_alias","value":9}""")).Value.AsInt() == 9);
});

Test("advanced compiler nodes add lazy control flow without central compiler edits", () =>
{
    var engine = new VmEngine(); engine.RegisterCompilerNode(new WhenNode());
    var script = Compile(engine, """{"operation":"when","condition":false,"value":{"variable":"never"},"fallback":42}""");
    var result = engine.Execute(script, options: explain);
    Check(result.Success && result.Value.AsInt() == 42);
    Check(!result.Explanation!.Tree!.Inputs.ContainsKey("value") && result.Explanation.Tree.Inputs.ContainsKey("fallback"));
    Check(engine.GetNodeCatalog().Nodes.Single(node => node.Operation == "when").Evaluation == "conditional");
});

Test("advanced compiler contexts close after compilation and enforce stack/output contracts", () =>
{
    var engine = new VmEngine();
    VmNodeCompilationContext? saved = null;
    engine.Nodes.RegisterCompiler(new() { Operation = "identity", Description = "Returns its child.", Inputs = [new() { Name = "value", Description = "Child expression." }] }, context => { saved = context; context.CompileInput("value"); });
    var script = Compile(engine, """{"operation":"identity","value":9}""");
    bool closed = false;
    try { saved!.PushConstant(DynValue.FromInt(10)); } catch (InvalidOperationException) { closed = true; }
    Check(closed && engine.Execute(script).Value.AsInt() == 9);
    engine.Nodes.RegisterCompiler(new() { Operation = "bad_stack", Description = "Invalid stack behavior." }, context => { context.PushConstant(DynValue.FromInt(1)); context.PushConstant(DynValue.FromInt(2)); });
    Check(engine.Execute(Compile(engine, """{"operation":"bad_stack"}""")).Error!.Code == "NODE_STACK_CONTRACT");
    engine.Nodes.RegisterCompiler(new() { Operation = "bad_compiled_output", Description = "Invalid output.", Output = new() { Types = ["bool"] } }, context => context.PushConstant(DynValue.FromInt(1)));
    Check(engine.Execute(Compile(engine, """{"operation":"bad_compiled_output"}""")).Error is VmTypeMismatchError { ExpectedType: "output:bool" });
});

Test("advanced compiler failures are source-aware compilation errors", () =>
{
    var engine = new VmEngine();
    engine.Nodes.RegisterCompiler(new() { Operation = "compiler_error", Description = "Deliberate compiler failure." }, _ => throw new ArgumentException("bad extension"));
    var result = engine.CompileFragment(JsonNode.Parse("""{"operation":"compiler_error"}"""), new() { RootPath = "/onAttack" });
    Check(result.Error!.Code == "NODE_COMPILER_ERROR" && result.Error.Source.Path == "/onAttack");
    var cycle = new List<DynValue>(); var cyclic = DynValue.FromArray(cycle); cycle.Add(cyclic);
    engine.Nodes.RegisterCompiler(new() { Operation = "cyclic_constant", Description = "Invalid cyclic constant." }, context => context.PushConstant(cyclic));
    Check(engine.CompileJson("""{"operation":"cyclic_constant"}""").Error!.Code == "NODE_COMPILER_ERROR");
});

Test("advanced compiler inputs enforce their declared types", () =>
{
    var engine = new VmEngine();
    engine.Nodes.RegisterCompiler(new() { Operation = "integer_identity", Description = "Returns a strict integer input.",
        Inputs = [new() { Name = "value", Description = "Integer expression.", AcceptedTypes = ["int"] }] }, context => context.CompileInput("value"));
    var result = engine.Execute(Compile(engine, """{"operation":"integer_identity","value":"wrong"}"""), options: explain);
    Check(result.Error is VmTypeMismatchError { ExpectedType: "int" } && result.Error.BytecodeInstruction.Opcode == "AssertNodeInput");
    Check(result.Error!.BytecodeInstruction.Inputs["value"].Value.GetString() == "wrong");
});

Test("custom-node bindings relocate when appended by legacy assembler", () =>
{
    var engine = new VmEngine(); engine.RegisterNode(new AddPercentNode());
    var vm = new VMRuntime.VM(); var assembler = new Assembler(vm.Program);
    assembler.Append(Compile(engine, """{"operation":"add_percent","value":100,"percent":25}"""));
    assembler.Append(Compile(engine, """{"operation":"add_percent","value":200,"percent":25}"""));
    vm.Run();
    Check(vm.Stack.Pull().AsFloat() == 250 && vm.Stack.Pull().AsFloat() == 125);
});

Test("callback failures retain pre-mutation bytecode operands", () =>
{
    var engine = new VmEngine();
    engine.RegisterNode(new() { Operation = "mutate_then_fail", Description = "Mutates input then throws.", Inputs = [new() { Name = "array", Description = "Mutable array.", AcceptedTypes = ["array"] }] }, args =>
    { args.GetInput("array").AsArray().Add(DynValue.FromInt(2)); throw new Exception("failure"); });
    var array = new List<DynValue> { DynValue.FromInt(1) };
    var result = engine.Execute(Compile(engine, """{"operation":"mutate_then_fail","array":{"variable":"a"}}"""), new Dictionary<string, DynValue> { ["a"] = DynValue.FromArray(array) });
    Check(array.Count == 2 && result.Error is VmHostCallError);
    Check(result.Error!.BytecodeInstruction.Inputs["array"].Value.GetArrayLength() == 1);
    Check(result.Error.JsonInstruction.Inputs["array"].Value.GetArrayLength() == 1);
});

Test("static collection parameters are isolated between custom-node invocations", () =>
{
    var engine = new VmEngine();
    using var json = JsonDocument.Parse("[1]");
    engine.RegisterNode(new()
    {
        Operation = "parameter_copy", Description = "Mutates its detached default.",
        Parameters = [new() { Name = "seed", Description = "Default array.", Required = false, AcceptedTypes = ["array"], DefaultValue = json.RootElement }],
        Output = new() { Types = ["int"] }
    }, args => { var list = args.GetParameter("seed").AsArray(); list.Add(DynValue.FromInt(2)); return DynValue.FromInt(list.Count); });
    var script = Compile(engine, """{"operation":"parameter_copy"}""");
    Check(engine.Execute(script).Value.AsInt() == 2 && engine.Execute(script).Value.AsInt() == 2);
});

Test("array construction enforces its collection size limit", () =>
{
    var engine = new VmEngine();
    var script = Compile(engine, """{"operation":"array","items":[1,2,3,4,5]}""");
    Check(engine.Execute(script, options: new() { MaxCollectionItems = 4 }).Error?.Code == "COLLECTION_LIMIT");
    var result = engine.Execute(script, options: new() { MaxCollectionItems = 5 });
    Check(result.Success && result.Value.AsArray().Select(v => v.AsInt()).SequenceEqual(new[] { 1, 2, 3, 4, 5 }));
});

Test("array construction charges slots and containers cumulatively", () =>
{
    var engine = new VmEngine();
    var script = Compile(engine, """{"operation":"array","items":[1,2,3,4,5]}""");
    Check(engine.Execute(script, options: new() { MaxAllocatedValues = 5 }).Error?.Code == "ALLOCATION_LIMIT");
    Check(engine.Execute(script, options: new() { MaxAllocatedValues = 6 }).Success);
    var repeated = Compile(engine, """{"operation":"sequence","steps":[{"operation":"array","items":[1,2,3]},{"operation":"array","items":[4,5,6]}]}""");
    Check(engine.Execute(repeated, options: new() { MaxAllocatedValues = 7 }).Error?.Code == "ALLOCATION_LIMIT");
    Check(engine.Execute(repeated, options: new() { MaxAllocatedValues = 8 }).Success);
    var empty = Compile(engine, """{"operation":"array","items":[]}""");
    Check(engine.Execute(empty, options: new() { MaxAllocatedValues = 1 }).Success);
});

int failed = 0;
foreach (var (name, run) in tests)
{
    try { run(); Console.WriteLine("PASS " + name); }
    catch (Exception error) { failed++; Console.Error.WriteLine("FAIL " + name + ": " + error); }
}
Console.WriteLine($"{tests.Count - failed}/{tests.Count} checks passed.");
return failed == 0 ? 0 : 1;

sealed class CollectingLogger : IVmLogger
{
    public List<VmError> Errors { get; } = new();
    public void Log(VmError error) => Errors.Add(error);
}
sealed class FakeHost : IVmHostObjects
{
    public DynValue Get(object target, string property) => DynValue.FromInt(1);
    public void Set(object target, string property, DynValue value) { }
    public DynValue Call(object target, string method, IReadOnlyList<DynValue> args)
        => DynValue.FromInt(args[0].AsInt() - args[1].AsInt());
    public bool IsValid(object target) => true;
    public void Free(object target) { }
    public IReadOnlyList<string> GetProperties(object target) => ["value"];
    public IReadOnlyList<string> GetMethods(object target) => ["subtract"];
}

sealed class LegacySmokeNode : global::VM.RuleNodeSystem.RuleNode
{
    public override void Compile(Assembler assembler)
        => assembler.Append(new VmJsonCompiler().Compile("13").Script!);
}

sealed class AddPercentNode : IVmJsonNode
{
    public VmNodeDefinition Definition => new()
    {
        Operation = "add_percent", Title = "Add percentage", Category = "project/math", Description = "Adds a percentage of the original value.", Aliases = ["boost"],
        Inputs = [new() { Name = "value", Description = "Original value.", AcceptedTypes = ["int", "float"] }, new() { Name = "percent", Description = "Percentage to add.", AcceptedTypes = ["int", "float"] }],
        Parameters = [new() { Name = "rounding", Description = "Optional rounding.", AcceptedTypes = ["string"], Required = false, DefaultValue = JsonSerializer.SerializeToElement("none"), EnumValues = ["none", "floor"] }],
        Output = new() { Types = ["float"], Description = "Adjusted value." },
        Examples = [JsonSerializer.SerializeToElement(new { operation = "add_percent", value = 100, percent = 25 })]
    };
    public DynValue Execute(VmNodeArguments args)
    {
        static float Number(DynValue value) => value.Type == VMRuntime.ValueType.Int ? value.AsInt() : value.AsFloat();
        float result = Number(args.GetInput("value")) * (1 + Number(args.GetInput("percent")) / 100f);
        return DynValue.FromFloat(args.GetParameter("rounding").AsString() == "floor" ? MathF.Floor(result) : result);
    }
}

sealed class WhenNode : IVmJsonNodeCompiler
{
    public VmNodeDefinition Definition => new()
    {
        Operation = "when", Description = "Evaluates only one of its value/fallback inputs.", Evaluation = "conditional",
        Inputs = [new() { Name = "condition", Description = "Strict Bool condition.", AcceptedTypes = ["bool"] }, new() { Name = "value", Description = "True branch." }, new() { Name = "fallback", Description = "False branch.", Required = false }]
    };
    public void Compile(VmNodeCompilationContext context)
    {
        context.CompileInput("condition");
        int alternate = context.Emit(OpcodeId.JumpIfFalse, b: 1);
        context.CompileInput("value");
        int end = context.Emit(OpcodeId.Jump);
        context.PatchJump(alternate, context.Position);
        context.CompileInput("fallback");
        context.PatchJump(end, context.Position);
    }
}
