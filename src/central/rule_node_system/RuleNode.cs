using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json.Nodes;

namespace VM.RuleNodeSystem
{
    public abstract class RuleNode
    {
        private static int _globalNodeId;
        public int NodeId { get; } = ++_globalNodeId;
        public string JsonPath { get; set; } = "";

        // Cache des métadonnées
        private RuleNodeMeta? _metaCache;
        public RuleNodeMeta GetMeta()
        {
            if (_metaCache == null)
                _metaCache = BuildMetaFromAttributes(GetType());
            return _metaCache;
        }

        public static RuleNodeMeta GetMetaStatic(Type type) => BuildMetaFromAttributes(type);

        private static RuleNodeMeta BuildMetaFromAttributes(Type type)
        {
            var meta = new RuleNodeMeta();
            var typeAttr = type.GetCustomAttribute<RuleNodeTypeAttribute>();
            if (typeAttr != null)
            {
                meta.TypeName = typeAttr.TypeName;
                meta.Description = typeAttr.Description;
                meta.Example = typeAttr.Example;
            }

            foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                var staticAttr = prop.GetCustomAttribute<StaticFieldAttribute>();
                if (staticAttr != null)
                    meta.StaticFields.Add(new FieldMeta
                    {
                        Name = staticAttr.Name,
                        Description = staticAttr.Description,
                        Required = staticAttr.Required,
                        AllowedValues = staticAttr.AllowedValues,
                        DefaultValue = staticAttr.DefaultValue,
                        SkipAutoValidation = staticAttr.SkipAutoValidation
                    });

                var inputAttr = prop.GetCustomAttribute<InputFieldAttribute>();
                if (inputAttr != null)
                    meta.InputFields.Add(new FieldMeta
                    {
                        Name = inputAttr.Name,
                        Description = inputAttr.Description,
                        Required = inputAttr.Required,
                        AcceptedTypes = inputAttr.AcceptedTypes,
                        SkipAutoValidation = inputAttr.SkipAutoValidation
                    });

                var variadicAttr = prop.GetCustomAttribute<VariadicInputAttribute>();
                if (variadicAttr != null)
                    meta.VariadicInputs.Add(new FieldMeta
                    {
                        Name = variadicAttr.Name,
                        Description = variadicAttr.Description,
                        Required = variadicAttr.Required,
                        AcceptedTypes = variadicAttr.AcceptedTypes,
                        SkipAutoValidation = variadicAttr.SkipAutoValidation
                    });
            }
            return meta;
        }

        // ---------------------------------------------------------------------
        // PARSING AUTOMATIQUE
        // ---------------------------------------------------------------------
        public virtual void Parse(JsonObject json)
        {
            var meta = GetMeta();
            var type = GetType();

            // Champs statiques
            foreach (var fieldMeta in meta.StaticFields)
            {
                var prop = FindPropertyByAttributeName<StaticFieldAttribute>(type, fieldMeta.Name);
                if (prop == null) continue;

                object? value = null;
                if (json.TryGetPropertyValue(fieldMeta.Name, out var jsonValue))
                {
                    value = ConvertJsonValue(jsonValue, prop.PropertyType);
                }
                else if (fieldMeta.DefaultValue != null)
                {
                    value = ConvertValue(fieldMeta.DefaultValue, prop.PropertyType);
                }
                else
                {
                    value = GetDefault(prop.PropertyType);
                }
                prop.SetValue(this, value);
            }

            // Inputs simples
            foreach (var fieldMeta in meta.InputFields)
            {
                var prop = FindPropertyByAttributeName<InputFieldAttribute>(type, fieldMeta.Name);
                if (prop == null) continue;

                if (json.TryGetPropertyValue("inputs", out var inputsNode) && inputsNode is JsonObject inputsObj)
                {
                    if (inputsObj.TryGetPropertyValue(fieldMeta.Name, out var inputJson))
                    {
                        var childPath = $"{JsonPath}.inputs.{fieldMeta.Name}";
                        var child = RuleNodeFactory.CreateFromJson(inputJson, childPath);
                        prop.SetValue(this, child);
                    }
                    else
                    {
                        // Champ manquant, sera traité par la validation
                        prop.SetValue(this, null);
                    }
                }
                else
                {
                    prop.SetValue(this, null);
                }
            }

            // Inputs variadiques
            foreach (var fieldMeta in meta.VariadicInputs)
            {
                var prop = FindPropertyByAttributeName<VariadicInputAttribute>(type, fieldMeta.Name);
                if (prop == null) continue;

                if (json.TryGetPropertyValue("variadicInputs", out var varInputsNode) && varInputsNode is JsonObject varInputsObj)
                {
                    if (varInputsObj.TryGetPropertyValue(fieldMeta.Name, out var varInputJson) && varInputJson is JsonArray arr)
                    {
                        var list = new List<RuleNode>();
                        for (int i = 0; i < arr.Count; i++)
                        {
                            var childPath = $"{JsonPath}.variadicInputs.{fieldMeta.Name}[{i}]";
                            var child = RuleNodeFactory.CreateFromJson(arr[i], childPath);
                            list.Add(child);
                        }
                        prop.SetValue(this, list);
                    }
                    else
                    {
                        prop.SetValue(this, null);
                    }
                }
                else
                {
                    prop.SetValue(this, null);
                }
            }
        }

        // ---------------------------------------------------------------------
        // VALIDATION GÉNÉRIQUE
        // ---------------------------------------------------------------------
        public virtual void Validate(ValidationContext context)
        {
            var meta = GetMeta();
            var type = GetType();

            // Validation des champs statiques
            foreach (var fieldMeta in meta.StaticFields)
            {
                if (fieldMeta.SkipAutoValidation) continue;
                var prop = FindPropertyByAttributeName<StaticFieldAttribute>(type, fieldMeta.Name);
                if (prop == null) continue;
                object? value = prop.GetValue(this);

                if (fieldMeta.Required && (value == null || (value is string s && string.IsNullOrEmpty(s))))
                    context.AddError(JsonPath, $"Missing required static field '{fieldMeta.Name}'.");

                if (value != null && fieldMeta.AllowedValues.Length > 0)
                {
                    string? strValue = value.ToString();
                    if (strValue != null && !fieldMeta.AllowedValues.Contains(strValue))
                        context.AddError(JsonPath, $"Value '{strValue}' for static field '{fieldMeta.Name}' is not allowed. Allowed: {string.Join(", ", fieldMeta.AllowedValues)}.");
                }
            }

            // Validation des inputs simples
            foreach (var fieldMeta in meta.InputFields)
            {
                if (fieldMeta.SkipAutoValidation) continue;
                var prop = FindPropertyByAttributeName<InputFieldAttribute>(type, fieldMeta.Name);
                if (prop == null) continue;
                object? value = prop.GetValue(this);

                if (fieldMeta.Required && value == null)
                    context.AddError(JsonPath, $"Missing required input '{fieldMeta.Name}'.");

                if (value is RuleNode child && fieldMeta.AcceptedTypes.Length > 0)
                {
                    string childType = child.GetMeta().TypeName;
                    if (!fieldMeta.AcceptedTypes.Contains(childType))
                        context.AddError(JsonPath, $"Input '{fieldMeta.Name}' type '{childType}' not accepted. Expected one of: {string.Join(", ", fieldMeta.AcceptedTypes)}.");
                }
            }

            // Validation des inputs variadiques
            foreach (var fieldMeta in meta.VariadicInputs)
            {
                if (fieldMeta.SkipAutoValidation) continue;
                var prop = FindPropertyByAttributeName<VariadicInputAttribute>(type, fieldMeta.Name);
                if (prop == null) continue;
                object? value = prop.GetValue(this);

                if (fieldMeta.Required && value == null)
                    context.AddError(JsonPath, $"Missing required variadic input '{fieldMeta.Name}'.");

                if (value is IReadOnlyList<RuleNode> list && fieldMeta.AcceptedTypes.Length > 0)
                {
                    foreach (var child in list)
                    {
                        string childType = child.GetMeta().TypeName;
                        if (!fieldMeta.AcceptedTypes.Contains(childType))
                            context.AddError(child.JsonPath, $"Element type '{childType}' not accepted in variadic input '{fieldMeta.Name}'. Expected one of: {string.Join(", ", fieldMeta.AcceptedTypes)}.");
                    }
                }
            }
        }

        // ---------------------------------------------------------------------
        // COMPILATION (à implémenter par les classes concrètes)
        // ---------------------------------------------------------------------
        public abstract void Compile(VMRuntime.Assembler assembler);

        // ---------------------------------------------------------------------
        // HELPERS
        // ---------------------------------------------------------------------
        private static PropertyInfo? FindPropertyByAttributeName<TAttribute>(Type type, string name)
            where TAttribute : Attribute
        {
            foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                var attr = prop.GetCustomAttribute<TAttribute>();
                if (attr != null)
                {
                    string attrName = attr switch
                    {
                        StaticFieldAttribute s => s.Name,
                        InputFieldAttribute i => i.Name,
                        VariadicInputAttribute v => v.Name,
                        _ => prop.Name
                    };
                    if (attrName == name) return prop;
                }
            }
            return null;
        }

        private static object? ConvertJsonValue(JsonNode? node, Type targetType)
        {
            if (node == null) return null;
            if (targetType.IsAssignableFrom(node.GetType())) return node;
            if (node is JsonValue jv)
            {
                object? raw = jv.GetValue<object?>();
                return ConvertValue(raw, targetType);
            }
            return ConvertValue(node.ToJsonString(), targetType);
        }

        private static object? ConvertValue(object? raw, Type targetType)
        {
            if (raw == null) return null;
            if (targetType.IsAssignableFrom(raw.GetType())) return raw;
            if (targetType.IsEnum && raw is string s) return Enum.Parse(targetType, s);
            return Convert.ChangeType(raw, targetType);
        }

        private static object? GetDefault(Type t) => t.IsValueType ? Activator.CreateInstance(t) : null;
    }

    public class RuleParseException : Exception
    {
        public RuleParseException(string message) : base(message) { }
    }
}