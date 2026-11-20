using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VMRuntime
{
    [Generator]
    public sealed class ExecuteInstructionGenerator : ISourceGenerator
    {
        private const string RuntimeNamespace = "VMRuntime";

        private const string IOpcodeFullName = $"{RuntimeNamespace}.IOpcode";
        private const string OpcodeIdFullName = $"{RuntimeNamespace}.OpcodeId";
        private const string OpcodeAttributeFullName = $"{RuntimeNamespace}.OpcodeAttribute";

        private const string GeneratedInstructionFileName = "VM_ExecuteInstruction.g.cs";
        private const string GeneratedAssemblerFileName = "Assembler.g.cs";

        private const string AssemblerFullName = $"{RuntimeNamespace}.Assembler";

        private static readonly SymbolDisplayFormat ShortTypeFormat =
            new(
                typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameOnly);

        private static readonly DiagnosticDescriptor MissingOpcodeAttribute =
        new(
            id: "VM001",
            title: "Missing Opcode attribute",
            messageFormat: "Type '{0}' implements IOpcode but is missing [Opcode(...)] attribute",
            category: "VMRuntime",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true
        );

        private static readonly DiagnosticDescriptor DuplicateOpcode =
        new(
            id: "VM002",
            title: "Duplicate opcode",
            messageFormat: "Opcode '{0}' is already implemented by '{1}'. Duplicate implementation found in '{2}'.",
            category: "VMRuntime",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true
        );

        public void Initialize(GeneratorInitializationContext context)
        {
            // Collect possible opcode implementations.
            context.RegisterForSyntaxNotifications(
                () => new OpcodeTypeReceiver());
        }


        public void Execute(GeneratorExecutionContext context)
        {
            if (context.SyntaxReceiver is not OpcodeTypeReceiver receiver)
                return;

            var compilation = context.Compilation;

            var opcodeInterface =
                compilation.GetTypeByMetadataName(IOpcodeFullName);

            var opcodeEnum =
                compilation.GetTypeByMetadataName(OpcodeIdFullName);

            if (opcodeInterface is null || opcodeEnum is null)
                return;


            var opcodeTypes = FindOpcodeTypes(
                context,
                compilation,
                receiver,
                opcodeInterface
            );

            if (opcodeTypes.Count == 0)
                return;


            ValidateDuplicateOpcodes(
                context,
                opcodeTypes);


            var source = GenerateExecuteInstruction(opcodeTypes);

            context.AddSource(
                GeneratedInstructionFileName,
                source);

            var assemblerSource = GenerateAssembler(compilation, opcodeTypes);
            if (!string.IsNullOrEmpty(assemblerSource))
                context.AddSource(GeneratedAssemblerFileName, assemblerSource);
        }

        private static void ValidateDuplicateOpcodes(
            GeneratorExecutionContext context,
            IReadOnlyCollection<INamedTypeSymbol> opcodeTypes)
        {
            var usedOpcodes = new Dictionary<string, INamedTypeSymbol>();

            foreach (var opcodeType in opcodeTypes)
            {
                var opcodeId = GetOpcodeEnumMember(opcodeType);

                if (opcodeId is null)
                    continue;


                if (usedOpcodes.TryGetValue(opcodeId, out var existingType))
                {
                    context.ReportDiagnostic(
                        Diagnostic.Create(
                            DuplicateOpcode,
                            opcodeType.Locations.FirstOrDefault(),
                            opcodeId,
                            existingType.Name,
                            existingType.Locations.FirstOrDefault()));

                    continue;
                }


                usedOpcodes.Add(
                    opcodeId,
                    opcodeType);
            }
        }

        private static List<INamedTypeSymbol> FindOpcodeTypes(
            GeneratorExecutionContext context,
            Compilation compilation,
            OpcodeTypeReceiver receiver,
            INamedTypeSymbol opcodeInterface)
        {
            var result = new List<INamedTypeSymbol>();

            foreach (var syntax in receiver.CandidateTypes)
            {
                var model = compilation.GetSemanticModel(
                    syntax.SyntaxTree);

                if (model.GetDeclaredSymbol(syntax)
                    is not INamedTypeSymbol type)
                {
                    continue;
                }


                if (!type.AllInterfaces.Contains(opcodeInterface))
                    continue;


                var hasOpcodeAttribute =
                    type.GetAttributes()
                        .Any(attribute =>
                            attribute.AttributeClass?
                                .ToDisplayString()
                                == OpcodeAttributeFullName);


                if (!hasOpcodeAttribute)
                {
                    context.ReportDiagnostic(
                        Diagnostic.Create(
                            MissingOpcodeAttribute,
                            type.Locations.FirstOrDefault(),
                            type.Name));

                    continue;
                }


                result.Add(type);
            }

            return result;
        }


        private static string GenerateAssembler(
    Compilation compilation,
    IEnumerable<INamedTypeSymbol> opcodeTypes)
        {
            var assemblerSymbol = compilation.GetTypeByMetadataName(AssemblerFullName);
            if (assemblerSymbol is null) return string.Empty;

            var sb = new StringBuilder();
            sb.AppendLine("// <auto-generated/>");
            sb.AppendLine();
            sb.AppendLine($"namespace {RuntimeNamespace}");
            sb.AppendLine("{");
            sb.AppendLine("    public partial class Assembler");
            sb.AppendLine("    {");

            foreach (var opcodeType in opcodeTypes)
            {
                // Bookkeeping instructions are compiler-owned, not public assembler entry points.
                if (opcodeType.DeclaredAccessibility != Accessibility.Public) continue;
                var opcodeId = GetOpcodeEnumMember(opcodeType);
                if (opcodeId is null) continue;

                var opcodeTypeName = opcodeType.ToDisplayString(ShortTypeFormat);

                // Chercher les méthodes Emit
                var emitMethods = opcodeType.GetMembers()
                    .OfType<IMethodSymbol>()
                    .Where(m => m.IsStatic && m.Name == "Emit" &&
                                m.Parameters.Length >= 1 &&
                                SymbolEqualityComparer.Default.Equals(m.Parameters[0].Type, assemblerSymbol))
                    .ToList();

                if (emitMethods.Count == 0)
                {
                    // Pas de Emit → génération d'une méthode sans argument
                    var methodName = GetAssemblerMethodName(opcodeType);
                    sb.AppendLine($"        /// <summary>Emits {methodName} and returns this assembler.</summary>");
                    sb.AppendLine($"        public Assembler {methodName}()");
                    sb.AppendLine("        {");
                    sb.AppendLine($"            AddInstruction(new Instruction {{ Opcode = {opcodeId} }});");
                    sb.AppendLine("            return this;");
                    sb.AppendLine("        }");
                }
                else
                {
                    // Une ou plusieurs méthodes Emit → générer pour chacune
                    for (int i = 0; i < emitMethods.Count; i++)
                    {
                        var emitMethod = emitMethods[i];
                        var methodName = i == 0
                            ? GetAssemblerMethodName(opcodeType)
                            : GetAssemblerMethodName(opcodeType) + i; // suffixe pour éviter les conflits

                        var parameters = emitMethod.Parameters.Skip(1).ToList();
                        var parameterList = string.Join(", ",
                            parameters.Select(p => $"{p.Type.ToDisplayString(ShortTypeFormat)} {p.Name}"));
                        var argumentList = string.Join(", ",
                            parameters.Select(p => p.Name));

                        sb.AppendLine($"        /// <summary>Emits {methodName} and returns this assembler.</summary>");
                        sb.AppendLine($"        public Assembler {methodName}({parameterList})");
                        sb.AppendLine("        {");
                        sb.AppendLine($"            {opcodeTypeName}.Emit(this{(argumentList.Length > 0 ? ", " + argumentList : "")});");
                        sb.AppendLine("            return this;");
                        sb.AppendLine("        }");
                    }
                }
            }

            sb.AppendLine("    }");
            sb.AppendLine("}");
            return sb.ToString();
        }

        // Petite aide pour extraire le nom de l'opcode (ex: AddOpcode → Add)
        private static string GetAssemblerMethodName(INamedTypeSymbol type)
        {
            var name = type.Name;
            if (name.EndsWith("Opcode", StringComparison.OrdinalIgnoreCase))
                name = name.Substring(0, name.Length - "Opcode".Length);
            return name;
        }


        private static string GenerateExecuteInstruction(
            IReadOnlyCollection<INamedTypeSymbol> opcodeTypes)
        {
            var builder = new StringBuilder();

            builder.AppendLine("// <auto-generated/>");
            builder.AppendLine();

            builder.AppendLine($"namespace {RuntimeNamespace}");
            builder.AppendLine("{");

            builder.AppendLine("    public partial class VM");
            builder.AppendLine("    {");

            builder.AppendLine(
                "        private partial int ExecuteInstruction(OpcodeId op, int currentIP)");

            builder.AppendLine("        {");


            builder.AppendLine("            return op switch");
            builder.AppendLine("            {");


            foreach (var opcodeType in opcodeTypes)
            {
                var opcodeId =
                    GetOpcodeEnumMember(opcodeType);


                if (opcodeId is null)
                    continue;


                var typeName =
                    opcodeType.ToDisplayString(
                        ShortTypeFormat);


                builder.AppendLine(
                    $"                {opcodeId} => {typeName}.Execute(this, currentIP),");
            }


            builder.AppendLine(
                "                _ => throw new System.InvalidOperationException($\"Unknown opcode: {op}\")");

            builder.AppendLine("            };");

            builder.AppendLine("        }");

            builder.AppendLine("    }");

            builder.AppendLine("}");

            return builder.ToString();
        }



        private static string? GetOpcodeEnumMember(
            INamedTypeSymbol type)
        {
            var attribute =
                type.GetAttributes()
                    .FirstOrDefault(attribute =>
                        attribute.AttributeClass?
                            .ToDisplayString()
                            == OpcodeAttributeFullName);


            if (attribute is null ||
                attribute.ConstructorArguments.Length != 1)
            {
                return null;
            }


            var argument =
                attribute.ConstructorArguments[0];


            if (argument.Kind != TypedConstantKind.Enum)
                return null;


            if (argument.Type is not INamedTypeSymbol enumSymbol)
                return null;


            var enumField =
                enumSymbol.GetMembers()
                    .OfType<IFieldSymbol>()
                    .FirstOrDefault(field =>
                        Equals(
                            field.ConstantValue,
                            argument.Value));


            if (enumField is null)
                return null;


            return $"OpcodeId.{enumField.Name}";
        }
    }



    /// <summary>
    /// Finds declarations that could implement IOpcode.
    /// </summary>
    internal sealed class OpcodeTypeReceiver : ISyntaxReceiver
    {
        public List<TypeDeclarationSyntax> CandidateTypes { get; } = new();



        public void OnVisitSyntaxNode(SyntaxNode node)
        {
            if (node is TypeDeclarationSyntax typeDeclaration &&
                typeDeclaration.BaseList?.Types.Count > 0)
            {
                CandidateTypes.Add(typeDeclaration);
            }
        }
    }
}
