extends SceneTree

var failures := 0

func _initialize() -> void:
    call_deferred("_run")

func _check(condition: bool, message: String) -> void:
    if not condition:
        failures += 1
        push_error(message)

func twice(value):
    return value * 2

func add_percent(inputs: Dictionary, parameters: Dictionary):
    var value = float(inputs["value"]) * (1.0 + float(inputs["percent"]) / 100.0)
    return floorf(value) if parameters["rounding"] == "floor" else value

func add_percent_v2(inputs: Dictionary, parameters: Dictionary):
    return add_percent(inputs, parameters) + 5.0

func _run() -> void:
    var module_path: String = get_script().resource_path.trim_suffix("tests/godot/smoke.gd")
    var engine = load(module_path + "src/godot/VmEngineNode.cs").new()
    engine.LogErrors = false
    var builtin_catalog = engine.GetNodeCatalog()
    _check(builtin_catalog.Nodes.size() == 41 and builtin_catalog.LiteralTypes.size() == 5, "Typed built-in catalog")
    _check(JSON.parse_string(engine.GetNodeCatalogJson())["format_version"] == 1, "Portable catalog format")
    var compilation = engine.CompileJson(
        '{"operation":"multiply","left":{"variable":"population"},"right":{"variable":"tax_rate"}}',
        "mods/tax.json"
    )
    _check(compilation.Success, "GDScript compilation failed")
    if not compilation.Success:
        print(compilation.ToJson())
        engine.free()
        quit(1)
        return

    var program = compilation.Script
    var plain = engine.Execute(program, {"population": 1000, "tax_rate": 0.15})
    _check(plain.Success and plain.Value == 150.0, "GDScript normal result")
    _check(plain.Explanation == null, "Normal mode should not create an explanation")

    var options = load(module_path + "src/godot/VmExecutionOptionsObject.cs").new()
    options.Explain = true
    var explained = engine.Execute(program, {"population": 2000, "tax_rate": 0.15}, options)
    _check(explained.Success and explained.GetValue() == 300.0, "GDScript explained result")
    _check(explained.Explanation.Tree.Inputs["left"].Node.Output.Value == 2000, "Typed explanation inputs")
    var portable = JSON.parse_string(explained.GetExplanationJson())
    _check(portable["tree"]["inputs"]["right"]["inputs"]["name"] == "tax_rate", "Portable explanation shape")

    var failed = engine.Execute(program, {}, options)
    _check(not failed.Success and failed.Error.Code == "VARIABLE_NOT_FOUND", "Typed error code")
    _check(failed.Error.VariableName == "population", "Specialized error property")
    _check(failed.Error.Source.Path == "/left", "Error JSON Pointer")
    _check(failed.Error.BytecodeInstruction.Opcode == "LoadVariable", "Bytecode failure context")
    _check(failed.Explanation.Tree.Inputs["left"].Node.Error.Code == failed.Error.Code, "Tree failure context")
    _check(JSON.parse_string(failed.Error.ToJson())["variable_name"] == "population", "Specialized JSON error")
    var failed_plain = engine.Execute(program, {})
    _check(not failed_plain.Success and failed_plain.Explanation == null, "Plain failure has no explanation")
    _check(failed_plain.Error.BytecodeInstruction.Inputs["name"].Value == "population", "Plain failure has typed inputs")

    var invalid = engine.Execute(program, {"population": 2147483648, "tax_rate": 0.15})
    _check(not invalid.Success and invalid.Error.Code == "INVALID_ARGUMENT", "Variant Int32 overflow is explicit")

    engine.RegisterFunction("twice", Callable(self, "twice"))
    var callback_script = engine.CompileJson('{"operation":"call","function":"twice","args":[21]}').Script
    var called = engine.Execute(callback_script)
    _check(called.Success and called.Value == 42, "GDScript callback arguments/results")

    var target := Node2D.new()
    target.name = "smoke_target"
    target.position = Vector2(7, 8)
    var get_script = engine.CompileJson(
        '{"operation":"object_get","target":{"variable":"target"},"property":"position"}'
    ).Script
    var got = engine.Execute(get_script, {"target": target})
    _check(got.Success and got.Value == Vector2(7, 8), "Godot object/vector adapter")
    var call_script = engine.CompileJson(
        '{"operation":"object_call","target":{"variable":"target"},"method":"get_name","args":[]}'
    ).Script
    var named = engine.Execute(call_script, {"target": target})
    _check(named.Success and named.Value == "smoke_target", "Godot exposed method call")
    target.free()

    var probe = load("res://PackageProbe.cs").new()
    _check(probe.ValidateDefaultMethods() == "", "Every default permission names an existing native method")
    var animation := AnimatedSprite2D.new()
    var timer := Timer.new()
    var body := RigidBody2D.new()
    root.add_child(timer)
    root.add_child(body)
    for spec in [[animation, "is_playing", []], [timer, "start", [1.0]], [body, "apply_impulse", [Vector2(1, 0)]]]:
        var args_json = [] if spec[1] != "start" else [1.0]
        if spec[1] == "apply_impulse":
            args_json = [{"variable": "impulse"}]
        var native_script = engine.CompileJson(JSON.stringify({"operation": "object_call", "target": {"variable": "target"}, "method": spec[1], "args": args_json})).Script
        var native_result = engine.Execute(native_script, {"target": spec[0], "impulse": Vector2(1, 0)})
        _check(native_result.Success, "Native default call: " + spec[1])
    _check(not timer.is_stopped(), "Default timer call actually started the timer")
    animation.free()
    timer.free()
    body.free()

    var if_script = engine.CompileJson(
        '{"operation":"if","condition":true,"then":1,"else":{"variable":"never"}}'
    ).Script
    var branched = engine.Execute(if_script, {}, options)
    _check(branched.Success and not branched.Explanation.Tree.Inputs.has("else"), "Unvisited branch is absent")

    var item = {
        "name": "Excalibur",
        "price": "10",
        "onEquip": {"operation": "unsupported_sibling"},
        "onAttack": {
            "operation": "multiply",
            "inputs": {
                "left": {"operation": "read_variable", "name": "hp_max_target"},
                "right": 10
            }
        }
    }
    var metadata = load(module_path + "src/godot/VmCompilationMetadataObject.cs").new()
    metadata.ScriptId = "items/sword:onAttack"
    metadata.SourceFile = "mods/items/sword.json"
    metadata.RootPath = "/items/sword/onAttack"
    metadata.Tags["item_id"] = "sword"
    var fragment_compilation = engine.CompileFragment(item["onAttack"], metadata)
    _check(fragment_compilation.Success, "Compile only onAttack from a parsed Dictionary")
    if fragment_compilation.Success:
        var attack = fragment_compilation.Script
        var damage = engine.Execute(attack, {"hp_max_target": 50}, options)
        _check(damage.Success and damage.Value == 500, "Selected item script result")
        _check(item["name"] == "Excalibur" and item["price"] == "10", "Item attributes remain data")
        _check(attack.IsFragment and attack.RootPath == metadata.RootPath, "Compiled fragment metadata")
        _check(attack.Metadata.Tags["item_id"] == "sword", "Typed metadata labels")
        _check(damage.Explanation.Tree.Inputs["left"].Node.Path == "/items/sword/onAttack/inputs/left", "Merged logical trace path")
        var unknown_position = engine.Execute(attack, {})
        _check(unknown_position.Error.Source.Line == 0 and not unknown_position.Error.Source.HasPosition, "No fabricated original line")

        var origin = load(module_path + "src/godot/VmSourceOriginObject.cs").new()
        origin.File = "mods/base/hit.json"
        origin.Path = "/effects/hp"
        origin.Line = 42
        origin.ByteColumn = 8
        metadata.SourceMap["/inputs/left"] = origin
        var located_attack = engine.CompileFragment(item["onAttack"], metadata).Script
        var missing_hp = engine.Execute(located_attack, {}, options)
        _check(missing_hp.Error.Source.File == "mods/base/hit.json" and missing_hp.Error.Source.Line == 42, "Original mod provenance")
        _check(missing_hp.Error.Source.Path == "/effects/hp", "Original source path")
        _check(missing_hp.Error.JsonInstruction.Path == "/items/sword/onAttack/inputs/left", "Logical JSON instruction path")
        _check(missing_hp.Explanation.Tree.Inputs["left"].Node.Source.Line == 42, "Original location in explanation")
        origin.Line = 99
        _check(engine.Execute(located_attack, {}).Error.Source.Line == 42, "Compiled metadata is immutable")
        var updated_attack = engine.CompileFragment(item["onAttack"], metadata).Script
        _check(updated_attack.MetadataHash != located_attack.MetadataHash, "Changed provenance invalidates metadata cache")
        _check(engine.Execute(updated_attack, {}).Error.Source.Line == 99, "Updated provenance is used")

    var float_fragment = engine.CompileFragment(10.0, metadata)
    _check(float_fragment.Success and typeof(engine.Execute(float_fragment.Script).Value) == TYPE_FLOAT, "Whole-valued Float stays Float")
    var text_fragment = engine.CompileFragmentJson("1.0", metadata)
    _check(text_fragment.Success and typeof(engine.Execute(text_fragment.Script).Value) == TYPE_FLOAT, "Fragment text API")
    var invalid_metadata = load(module_path + "src/godot/VmCompilationMetadataObject.cs").new()
    invalid_metadata.RootPath = "onAttack"
    _check(engine.CompileFragment(1, invalid_metadata).Error.Code == "INVALID_METADATA", "Typed invalid fragment metadata")

    var cyclic_script = engine.CompileJson(
        '{"operation":"array_push","array":{"variable":"a"},"value":{"variable":"a"}}'
    ).Script
    var cyclic = engine.Execute(cyclic_script, {"a": []}, options)
    _check(not cyclic.Success and cyclic.Error.Code == "RESULT_CONVERSION", "Cyclic output is a structured boundary error")
    _check(cyclic.Explanation.Tree.Error.Code == "RESULT_CONVERSION", "Boundary error remains visible in explanation")

    var definition = load(module_path + "src/godot/VmNodeDefinitionObject.cs").new()
    definition.Operation = "add_percent"
    definition.Title = "Add percentage"
    definition.Category = "project/math"
    definition.Description = "Adds a percentage of the original value."
    definition.Aliases = ["boost"]
    definition.Output.Types = ["float"]
    definition.Output.Description = "Adjusted value."
    definition.ExamplesJson = '[{"operation":"add_percent","value":100,"percent":25}]'
    for input_name in ["value", "percent"]:
        var input = load(module_path + "src/godot/VmNodeInputDefinitionObject.cs").new()
        input.Name = input_name
        input.Description = "Numeric " + input_name
        input.AcceptedTypes = ["int", "float"]
        definition.Inputs.append(input)
    var rounding = load(module_path + "src/godot/VmNodeParameterDefinitionObject.cs").new()
    rounding.Name = "rounding"
    rounding.Description = "Optional rounding mode."
    rounding.AcceptedTypes = ["string"]
    rounding.Required = false
    rounding.DefaultValueJson = '"none"'
    rounding.EnumValues = ["none", "floor"]
    definition.Parameters.append(rounding)
    var registration = engine.RegisterNode(definition, Callable(self, "add_percent"))
    _check(registration.Success, "GDScript custom node registration")
    if not registration.Success:
        print(registration.ToJson())
    else:
        var catalog = engine.GetNodeCatalog()
        _check(catalog.Nodes.size() == 42 and catalog.RegistryVersion > builtin_catalog.RegistryVersion, "Project node appears in actual catalog")
        var found = false
        for node in catalog.Nodes:
            if node.Operation == "add_percent":
                found = node.Inputs.size() == 2 and node.Parameters[0].HasDefault and node.Origin == "project"
        _check(found, "Typed custom node documentation")
        var custom_source = '{"operation":"boost","value":100,"percent":25}'
        var custom_script = engine.CompileJson(custom_source).Script
        var custom_result = engine.Execute(custom_script, {}, options)
        _check(custom_result.Success and custom_result.Value == 125.0, "Custom callback execution")
        _check(custom_result.Explanation.Tree.Operation == "add_percent", "Canonical custom operation in explanation")
        _check(custom_result.Explanation.RegistryHash == catalog.RegistryHash, "Trace catalog identity")
        _check(JSON.parse_string(custom_result.GetExplanationJson())["tree"]["inputs"]["rounding"] == "none", "Static parameters in custom explanation")
        var replacement = engine.RegisterNode(definition, Callable(self, "add_percent_v2"), true)
        _check(replacement.Success, "Explicit project node replacement")
        var updated_script = engine.CompileJson(custom_source).Script
        _check(updated_script.RegistryVersion != custom_script.RegistryVersion, "Changed registry invalidates compiled cache")
        _check(custom_script.GetNodeCatalog().RegistryVersion == custom_script.RegistryVersion, "Compiled handle retains its catalog snapshot")
        _check(JSON.parse_string(custom_script.GetNodeCatalogJson())["registry_version"] == custom_script.RegistryVersion, "Old catalog remains exportable")
        _check(engine.Execute(custom_script).Value == 125.0 and engine.Execute(updated_script).Value == 130.0, "Old handles keep their bound implementation")
        var invalid_definition = load(module_path + "src/godot/VmNodeDefinitionObject.cs").new()
        invalid_definition.Operation = "multiply"
        invalid_definition.Description = "Cannot override a built-in."
        var invalid_registration = engine.RegisterNode(invalid_definition, Callable(self, "add_percent"), true)
        _check(not invalid_registration.Success and invalid_registration.Error.Code == "NODE_REGISTRATION", "Typed registration error")

    print("GDScript smoke checks: ", "PASS" if failures == 0 else "FAIL (%d)" % failures)
    engine.free()
    quit(0 if failures == 0 else 1)
