# Graph Report - Resolver  (2026-11-26)

## Corpus Check
- 152 files · ~47,459 words
- Verdict: corpus is large enough that graph structure adds value.
- Unclassified: 52 file(s) not represented in the graph (top: .uid 43, (none) 5, .props 2)
- WARNING: 52 of 204 files seen (25%) have no supported extractor and are missing from this graph entirely. It may look complete while representing only part of the codebase.

## Summary
- 1560 nodes · 3677 edges · 86 communities (60 shown, 26 thin omitted)
- Extraction: 86% EXTRACTED · 14% INFERRED · 0% AMBIGUOUS · INFERRED: 528 edges (avg confidence: 0.82)
- Token cost: 0 input · 0 output

## Graph Freshness
- Built from commit: `c094c26d`
- Run `git rev-parse HEAD` and compare to check if the graph is stale.
- Run `graphify update .` after code changes (no API cost).

## Community Hubs (Navigation)
- VM
- system_collections_generic
- ScriptColor
- OpcodeId
- Program
- DynValue
- Operators
- VmJsonObject
- ValueType
- publish
- Compilation.Expression
- VmEngine
- system_runtime_compilerservices
- Type
- IOpcode
- VmNodeRegistry
- VmEngineNode
- ExecuteInstructionGenerator
- RuleNodeMeta
- Resolver : compilation de fragments JSON et VM de modding
- VmTraceNode
- VmNodeDefinition
- VmTraceNodeObject
- DefaultExposedMethods.RegisterDefaults
- VMRuntime.Logging
- VmNodeDefinitionObject
- VmNodeInputDefinition
- CompiledScript
- VmNodeInfo
- VmValueSnapshot
- VMRuntime
- VmError
- VmNodeCompilationContext
- VmCompiledScriptObject
- VmSourceLocationObject
- VmExplanation
- VmNodeParameterDefinitionObject
- StaticFieldAttribute
- Audit du dépôt Resolver
- Resolver.Core.csproj
- Dictionary
- Func
- VmNodeCatalog
- VmNodeParameterDefinition
- Initializer.Initialize
- VmNodeOutputDefinition
- VmNodeKind
- VmExplanationObject
- RuleNodeProcessor.ProcessJson
- RuleNode
- RefCounted
- VmNodeSyntax
- IVmHostObjects
- VmExecutionResultObject
- VmNodeInputDefinitionObject
- VariadicInputAttribute
- GodotHostObjects
- VmNodeCatalogObject
- IReadOnlyList
- VmSourceLocation
- ConsoleLogSystem
- UnavailableHostObjects
- VmCompilationMetadataObject
- VMException
- GodotLogSystem
- InputFieldAttribute
- ValidationContext
- ILogSystem
- VmFragmentJson.Write
- Resolver.Godot.Smoke.csproj
- AGENTS.md
- ArrayConcatOpcode.cs
- GeOpcode.cs
- GtOpcode.cs
- LeOpcode.cs
- NewDictOpcode.cs
- ClampOpcode.cs
- DivOpcode.cs
- MaxOpcode.cs
- ModOpcode.cs
- NegOpcode.cs
- NotOpcode.cs
- GodotJsonFragment.Convert

## God Nodes (most connected - your core abstractions)
1. `DynValue` - 123 edges
2. `VM` - 86 edges
3. `VMRuntime` - 84 edges
4. `OpcodeId` - 57 edges
5. `ValueType` - 52 edges
6. `IOpcode` - 50 edges
7. `VmNodeDefinition` - 39 edges
8. `CompiledScript` - 38 edges
9. `VMRuntime.Diagnostics` - 35 edges
10. `VMRuntime.GodotApi` - 32 edges

## Surprising Connections (you probably didn't know these)
- `Architecture et contribution` --references--> `VmJsonCompiler`  [INFERRED]
  docs/ARCHITECTURE.md → src/central/compiler/VmJsonCompiler.cs
- `Frontières` --references--> `VmNodeRegistry`  [INFERRED]
  docs/ARCHITECTURE.md → src/central/nodes/VmNodeRegistry.cs
- `Où intervenir` --references--> `RuleNodeProcessor`  [INFERRED]
  docs/ARCHITECTURE.md → src/central/rule_node_system/RuleNodeProcessor.cs
- `Architecture et contribution` --references--> `CompiledScript`  [INFERRED]
  docs/ARCHITECTURE.md → src/central/runtime/Program.cs
- `Architecture : ce qui fonctionne bien` --references--> `RuleNodeProcessor`  [INFERRED]
  AUDIT_REPO.md → src/central/rule_node_system/RuleNodeProcessor.cs

## Import Cycles
- None detected.

## Communities (86 total, 26 thin omitted)

### Community 0 - "VM"
Cohesion: 0.07
Nodes (16): VmRuntimeFault, Code, Detail, Stack, VM, Host, VmExecutionOptions, Explain (+8 more)

### Community 1 - "system_collections_generic"
Cohesion: 0.06
Nodes (34): VMRuntime.Diagnostics, VMRuntime.GodotApi, VmNodeRegistrationResult, Error, Success, VmBytecodeInstructionObject, InstructionIndex, Opcode (+26 more)

### Community 2 - "ScriptColor"
Cohesion: 0.05
Nodes (7): ValueUnion, ScriptColor, ScriptVector2, ScriptVector2I, ScriptVector3, ScriptVector3I, GodotScriptConversions

### Community 3 - "OpcodeId"
Cohesion: 0.04
Nodes (49): OpcodeId, Add, ArrayConcat, ArrayContains, ArrayGet, ArrayLength, ArrayPush, ArrayRemoveAt (+41 more)

### Community 4 - "Program"
Cohesion: 0.09
Nodes (18): Assembler, ArrayContainsOpcode, ArrayGetOpcode, ArrayRemoveAtOpcode, ArraySetOpcode, MakeArrayOpcode, JumpIfFalseOpcode, JumpIfTrueOpcode (+10 more)

### Community 5 - "DynValue"
Cohesion: 0.11
Nodes (10): VmNodeArguments, Inputs, Parameters, DynValue, IsArray, IsDict, IsNull, RefValue (+2 more)

### Community 6 - "Operators"
Cohesion: 0.08
Nodes (12): EqualOperator, NotEqualOperator, GreaterOrEqualOperator, GreaterThanOperator, LessOrEqualOperator, LessThanOperator, AdditionOperator, DivisionOperator (+4 more)

### Community 7 - "VmJsonObject"
Cohesion: 0.07
Nodes (28): VmBytecodeInstructionContext, Inputs, InstructionIndex, Opcode, Operands, VmDivisionByZeroError, VmExecutionError, BytecodeInstruction (+20 more)

### Community 8 - "ValueType"
Cohesion: 0.07
Nodes (20): ConversionTable, ValueConverterFn, ModuloOperator, NegateOperator, NotOperator, UnaryOperator, ValueType, Array (+12 more)

### Community 9 - "publish"
Cohesion: 0.07
Nodes (33): NUGET_API_KEY, jobs, package, publish, name, on, pull_request, push (+25 more)

### Community 10 - "Compilation.Expression"
Cohesion: 0.14
Nodes (13): LocatedJson, File, Root, Compilation, Program, VmCompilationError, Kind, VmCompileFault (+5 more)

### Community 11 - "VmEngine"
Cohesion: 0.14
Nodes (14): VmCompilationMetadata, EffectiveId, RootPath, ScriptId, SourceFile, SourceMap, Tags, VmJsonCompiler (+6 more)

### Community 12 - "system_runtime_compilerservices"
Cohesion: 0.06
Nodes (12): ArrayLengthOpcode, ArrayPushOpcode, NewArrayOpcode, EqOpcode, InRangeOpcode, LtOpcode, NeqOpcode, JumpOpcode (+4 more)

### Community 13 - "Type"
Cohesion: 0.15
Nodes (6): VmJson, ErrorConverter, ErrorConverterFactory, TraceConverterFactory, TraceInputConverter, ExposedMethodRegistry

### Community 14 - "IOpcode"
Cohesion: 0.10
Nodes (18): IOpcode, AssertBoolOpcode, AssertNodeInputOpcode, CallFunctionOpcode, EnterNodeOpcode, ExitNodeOpcode, InvokeNodeOpcode, LoadVariableOpcode (+10 more)

### Community 15 - "VmNodeRegistry"
Cohesion: 0.12
Nodes (8): Architecture et contribution, Frontières, Valider une modification, IVmJsonNodeCompiler, Definition, VmNodeRegistration, VmNodeRegistry, VmNodeRegistrySnapshot

### Community 16 - "VmEngineNode"
Cohesion: 0.15
Nodes (6): VmCompilationResultObject, Success, VmEngineNode, CoreEngine, GodotVmLogger, LogErrors

### Community 17 - "ExecuteInstructionGenerator"
Cohesion: 0.14
Nodes (3): ExecuteInstructionGenerator, OpcodeTypeReceiver, CandidateTypes

### Community 18 - "RuleNodeMeta"
Cohesion: 0.09
Nodes (20): VM.RuleNodeSystem, FieldMeta, AcceptedTypes, AllowedValues, DefaultValue, Description, Name, Required (+12 more)

### Community 19 - "Resolver : compilation de fragments JSON et VM de modding"
Cohesion: 0.08
Nodes (24): Ajouter un nœud de contrôle de flux en C#, Ajouter un nœud depuis GDScript, Ajouter un nœud en C#, Cache et changement du JSON, Catalogue JSON des nœuds disponibles, Compiler seulement onAttack depuis GDScript, Contrat du catalogue, format_version = 1, Distribution NuGet (+16 more)

### Community 20 - "VmTraceNode"
Cohesion: 0.13
Nodes (18): VmTraceChild, Node, VmTraceInput, VmTraceLiteral, Value, VmTraceNode, Error, ExecutionId (+10 more)

### Community 21 - "VmNodeDefinition"
Cohesion: 0.11
Nodes (14): VmNodeDefinition, Aliases, Category, Deprecated, Description, Evaluation, Examples, Inputs (+6 more)

### Community 22 - "VmTraceNodeObject"
Cohesion: 0.12
Nodes (13): Tree, VmTraceInputObject, Kind, Node, Occurrences, VmTraceNodeObject, ExecutionId, Inputs (+5 more)

### Community 24 - "VMRuntime.Logging"
Cohesion: 0.13
Nodes (5): VMRuntime.Logging, VMRuntime.Errors, net10.0, Godot.NET.Sdk/4.7.2, LegacySmokeNode

### Community 25 - "VmNodeDefinitionObject"
Cohesion: 0.13
Nodes (14): VmNodeDefinitionObject, Aliases, Category, Deprecated, Description, Evaluation, ExamplesJson, Inputs (+6 more)

### Community 26 - "VmNodeInputDefinition"
Cohesion: 0.15
Nodes (12): VmBuiltinNodes, LiteralTypes, VmNodeInputDefinition, AcceptedTypes, DefaultValue, Description, HasDefault, MinItems (+4 more)

### Community 27 - "CompiledScript"
Cohesion: 0.11
Nodes (18): CompiledScript, Bytecode, CompilerVersion, FormatVersion, Id, InstructionCount, IsFragment, Metadata (+10 more)

### Community 28 - "VmNodeInfo"
Cohesion: 0.11
Nodes (16): VmNodeInfo, Id, InputNames, InputTypes, Operation, OutputTypes, Parameters, ParentInput (+8 more)

### Community 30 - "VmValueSnapshot"
Cohesion: 0.14
Nodes (11): VmValueSnapshot, Truncated, Type, Value, Inputs, Literal, Output, VmValueSnapshotObject (+3 more)

### Community 32 - "VmError"
Cohesion: 0.15
Nodes (11): VmError, Code, Kind, Message, Source, IVmLogger, NullVmLogger, Instance (+3 more)

### Community 33 - "VmNodeCompilationContext"
Cohesion: 0.19
Nodes (6): VmNodeCompilationContext, Definition, Parameters, Position, WhenNode, Definition

### Community 34 - "VmCompiledScriptObject"
Cohesion: 0.13
Nodes (10): VmCompiledScriptObject, Id, InstructionCount, IsFragment, MetadataHash, RegistryHash, RegistryVersion, RootPath (+2 more)

### Community 35 - "VmSourceLocationObject"
Cohesion: 0.14
Nodes (12): VmErrorObject, Code, Kind, Message, Source, VmSourceLocationObject, ByteColumn, File (+4 more)

### Community 36 - "VmExplanation"
Cohesion: 0.12
Nodes (14): VmExplanation, FormatVersion, IsFragment, MetadataHash, RegistryHash, RegistryVersion, RootPath, Script (+6 more)

### Community 37 - "VmNodeParameterDefinitionObject"
Cohesion: 0.16
Nodes (10): NodeDefinitionJson, VmNodeParameterDefinitionObject, AcceptedTypes, DefaultValueJson, Description, EnumValues, HasDefault, MinLength (+2 more)

### Community 38 - "StaticFieldAttribute"
Cohesion: 0.13
Nodes (10): ExposedToVM, OpcodeAttribute, Id, StaticFieldAttribute, AllowedValues, DefaultValue, Description, Name (+2 more)

### Community 39 - "Audit du dépôt Resolver"
Cohesion: 0.13
Nodes (15): A1 — P1 — Plusieurs méthodes Godot autorisées par défaut sont inutilisables, A2 — P2 — Le README étend trop la portée des limites de taille, A3 — P2 — Les capacités accordées aux objets Godot doivent être explicites, A4 — P2 avant publication publique — Licence et métadonnées à décider, A5 — P3 — La procédure de montée de version oublie le workflow, A6 — P3 — La vérification locale d’un paquet peut réutiliser une ancienne version, Audit du dépôt Resolver, Avis général (+7 more)

### Community 40 - "Resolver.Core.csproj"
Cohesion: 0.13
Nodes (11): net10.0, Microsoft.NET.Sdk, netstandard2.0, Microsoft.NET.Sdk, Darkelle.Resolver.Core ([$(Version)]), Microsoft.CodeAnalysis.Analyzers (3.3.4), Microsoft.CodeAnalysis.CSharp (4.8.0), net10.0 (+3 more)

### Community 41 - "Dictionary"
Cohesion: 0.15
Nodes (7): VmExecutionOptionsObject, Explain, MaxAllocatedValues, MaxCollectionItems, MaxInstructions, MaxStringLength, MaxTraceSteps

### Community 42 - "Func"
Cohesion: 0.15
Nodes (5): IVmJsonNode, Definition, VmNodeCallBinding, AddPercentNode, Definition

### Community 43 - "VmNodeCatalog"
Cohesion: 0.14
Nodes (12): VmLiteralTypeDefinition, Components, Description, Tag, VmNodeCatalog, FormatVersion, LiteralTypes, Nodes (+4 more)

### Community 44 - "VmNodeParameterDefinition"
Cohesion: 0.13
Nodes (13): VmNodeParameterDefinition, AcceptedTypes, DefaultValue, Description, EnumValues, HasDefault, MinLength, Name (+5 more)

### Community 45 - "Initializer.Initialize"
Cohesion: 0.18
Nodes (5): Initializer, GodotPrintSystem, IPrintSystem, PrintManager, Current

### Community 46 - "VmNodeOutputDefinition"
Cohesion: 0.16
Nodes (9): VmNodeOutputDefinition, Description, Inference, Types, Nodes, VmNodeOutputDefinitionObject, Description, Inference (+1 more)

### Community 47 - "VmNodeKind"
Cohesion: 0.14
Nodes (14): VmNodeKind, And, Array, Call, Callback, Constant, CustomCompiler, If (+6 more)

### Community 48 - "VmExplanationObject"
Cohesion: 0.15
Nodes (12): VmExplanationObject, FormatVersion, IsFragment, MetadataHash, RegistryHash, RegistryVersion, RootPath, Script (+4 more)

### Community 49 - "RuleNodeProcessor.ProcessJson"
Cohesion: 0.29
Nodes (4): RuleParseException, RuleNodeFactory, VmCompilationException, Error

### Community 50 - "RuleNode"
Cohesion: 0.26
Nodes (3): RuleNode, JsonPath, NodeId

### Community 51 - "RefCounted"
Cohesion: 0.18
Nodes (7): VmSourceOriginObject, ByteColumn, File, Line, Path, PackageProbe, EngineType

### Community 52 - "VmNodeSyntax"
Cohesion: 0.15
Nodes (13): VmNodeSyntax, AllowDirectInputs, AllowParametersInInputContainer, AllowUnknownFields, CanonicalParameterPlacement, FieldNamesCaseSensitive, InputContainer, InputOrder (+5 more)

### Community 54 - "VmExecutionResultObject"
Cohesion: 0.21
Nodes (3): VmExecutionResultObject, Success, Value

### Community 55 - "VmNodeInputDefinitionObject"
Cohesion: 0.18
Nodes (10): VmNodeInputDefinitionObject, AcceptedTypes, DefaultValueJson, Description, HasDefault, MinItems, Name, NullAsEmpty (+2 more)

### Community 56 - "VariadicInputAttribute"
Cohesion: 0.18
Nodes (9): Architecture : ce qui fonctionne bien, Où intervenir, RuleNodeProcessor, VariadicInputAttribute, AcceptedTypes, Description, Name, Required (+1 more)

### Community 58 - "VmNodeCatalogObject"
Cohesion: 0.20
Nodes (6): VmNodeCatalogObject, FormatVersion, LiteralTypes, RegistryHash, RegistryVersion, ValueTypes

### Community 60 - "VmSourceLocation"
Cohesion: 0.20
Nodes (7): VmSourceLocation, ByteColumn, File, HasPosition, Line, Path, Metadata

### Community 63 - "VmCompilationMetadataObject"
Cohesion: 0.29
Nodes (6): VmCompilationMetadataObject, RootPath, ScriptId, SourceFile, SourceMap, Tags

### Community 66 - "InputFieldAttribute"
Cohesion: 0.29
Nodes (6): InputFieldAttribute, AcceptedTypes, Description, Name, Required, SkipAutoValidation

### Community 67 - "ValidationContext"
Cohesion: 0.43
Nodes (4): ValidationContext, Errors, HasErrors, ValidationError

### Community 68 - "ILogSystem"
Cohesion: 0.40
Nodes (3): ILogSystem, LogManager, Current

### Community 70 - "Resolver.Godot.Smoke.csproj"
Cohesion: 0.50
Nodes (3): Darkelle.Resolver.Godot ([$(Version)]), net10.0, Godot.NET.Sdk/4.7.2

## Knowledge Gaps
- **434 isolated node(s):** `net10.0`, `Godot.NET.Sdk/4.7.2`, `net10.0`, `Microsoft.NET.Sdk`, `CandidateTypes` (+429 more)
  These have ≤1 connection - possible missing edges or undocumented components. (Counts symbols only; 581 node(s) total have ≤1 connection when file, concept and rationale nodes are included.)
- **26 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `VMRuntime` connect `VMRuntime` to `system_collections_generic`, `ScriptColor`, `OpcodeId`, `Program`, `Operators`, `ValueType`, `system_runtime_compilerservices`, `IOpcode`, `VMRuntime.Logging`, `VmNodeCompilationContext`, `StaticFieldAttribute`, `VmNodeCatalog`, `IVmHostObjects`, `ArrayConcatOpcode.cs`, `GeOpcode.cs`, `GtOpcode.cs`, `LeOpcode.cs`, `NewDictOpcode.cs`, `ClampOpcode.cs`, `DivOpcode.cs`, `MaxOpcode.cs`, `ModOpcode.cs`, `NegOpcode.cs`, `NotOpcode.cs`?**
  _High betweenness centrality (0.122) - this node is a cross-community bridge._
- **What connects `net10.0`, `Godot.NET.Sdk/4.7.2`, `net10.0` to the rest of the system?**
  _434 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `VM` be split into smaller, more focused modules?**
  _Cohesion score 0.06662859318484675 - nodes in this community are weakly interconnected._
- **Why does `DynValue` connect `DynValue` to `VM`, `VmNodeCompilationContext`, `ScriptColor`, `Program`, `Operators`, `VmJsonObject`, `ValueType`, `Compilation.Expression`, `Func`, `VmEngine`, `VmNodeRegistry`, `IVmHostObjects`, `GodotHostObjects`, `IReadOnlyList`, `ConversionTable.ConversionTable`, `UnavailableHostObjects`?**
  _High betweenness centrality (0.121) - this node is a cross-community bridge._
- **Should `system_collections_generic` be split into smaller, more focused modules?**
  _Cohesion score 0.06017478152309613 - nodes in this community are weakly interconnected._
- **Why does `VM` connect `VM` to `VmExplanation`, `Program`, `DynValue`, `VmJsonObject`, `CompiledScript`, `Dictionary`, `Func`, `VmNodeRegistry`, `IVmHostObjects`, `VMRuntime.Logging`, `IReadOnlyList`, `VmNodeInfo`, `VmValueSnapshot`?**
  _High betweenness centrality (0.102) - this node is a cross-community bridge._
- **Should `ScriptColor` be split into smaller, more focused modules?**
  _Cohesion score 0.04915824915824916 - nodes in this community are weakly interconnected._