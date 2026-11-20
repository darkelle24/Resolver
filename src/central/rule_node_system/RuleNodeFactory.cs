using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.Json.Nodes;

namespace VM.RuleNodeSystem
{
    public static class RuleNodeFactory
    {
        private static readonly Dictionary<string, Func<RuleNode>> _creators = new();
        internal static bool IsRegistered(string typeName) => _creators.ContainsKey(typeName);

        static RuleNodeFactory()
        {
            // Enregistre automatiquement toutes les classes portant [RuleNodeType] dans l'assembly courant
            RegisterFromAssembly(Assembly.GetExecutingAssembly());
        }

        public static void RegisterFromAssembly(Assembly assembly)
        {
            foreach (var type in assembly.GetTypes())
            {
                var attr = type.GetCustomAttribute<RuleNodeTypeAttribute>();
                if (attr != null && typeof(RuleNode).IsAssignableFrom(type))
                {
                    var ctor = type.GetConstructor(Type.EmptyTypes);
                    if (ctor == null)
                        throw new InvalidOperationException($"{type.Name} must have a parameterless constructor.");
                    _creators[attr.TypeName] = () => (RuleNode)ctor.Invoke(null);
                }
            }
        }

        public static void Register(string typeName, Func<RuleNode> creator)
        {
            _creators[typeName] = creator;
        }

        /// <summary>
        /// Crée un RuleNode à partir d'un nœud JSON (objet ou valeur primitive).
        /// Les primitives sont automatiquement converties en ConstNode.
        /// </summary>
        public static RuleNode CreateFromJson(JsonNode? jsonNode, string jsonPath)
        {
            if (jsonNode is JsonObject jsonObj)
            {
                if (!jsonObj.TryGetPropertyValue("type", out var typeNode) || typeNode == null)
                    throw new RuleParseException($"Missing 'type' at {jsonPath}.");
                string typeName = typeNode.GetValue<string>();
                var node = Create(typeName, jsonPath);
                node.Parse(jsonObj); // parsing automatique
                return node;
            }
            else if (jsonNode is JsonValue jsonValue)
            {
                // Primitive → ConstNode
                object? value = jsonValue.GetValue<object?>();
                var constNode = Create("Const", jsonPath);
                // On suppose que ConstNode possède une propriété publique Value (via attribut StaticField)
                var prop = constNode.GetType().GetProperty("Value", BindingFlags.Public | BindingFlags.Instance);
                if (prop != null && prop.CanWrite)
                    prop.SetValue(constNode, value);
                else
                    throw new InvalidOperationException("ConstNode must have a writable 'Value' property.");
                return constNode;
            }
            else if (jsonNode == null)
            {
                // null → ConstNode avec valeur null
                var constNode = Create("Const", jsonPath);
                var prop = constNode.GetType().GetProperty("Value", BindingFlags.Public | BindingFlags.Instance);
                prop?.SetValue(constNode, null);
                return constNode;
            }
            else
            {
                throw new RuleParseException($"Unsupported JSON node type at {jsonPath}.");
            }
        }

        public static RuleNode Create(string typeName, string jsonPath)
        {
            if (!_creators.TryGetValue(typeName, out var creator))
                throw new RuleParseException($"Unknown rule node type '{typeName}' at {jsonPath}.");
            var node = creator();
            node.JsonPath = jsonPath;
            return node;
        }
    }
}
