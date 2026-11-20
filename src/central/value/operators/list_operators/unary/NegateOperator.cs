
using VMRuntime;
using ValueType = VMRuntime.ValueType;

public class NegateOperator : UnaryOperator
{
    protected override DynValue Evaluate(ValueType targetType, in DynValue operand)
    {
        return targetType switch
        {
            ValueType.Int => DynValue.FromInt(-operand.Data.IntValue),
            ValueType.Float => DynValue.FromFloat(-operand.Data.FloatValue),
            ValueType.Vector2 => DynValue.FromVector2(-operand.Data.Vector2Value),
            ValueType.Vector2i => DynValue.FromVector2i(-operand.Data.Vector2iValue),
            ValueType.Vector3 => DynValue.FromVector3(-operand.Data.Vector3Value),
            ValueType.Vector3i => DynValue.FromVector3i(-operand.Data.Vector3iValue),
            ValueType.Color => DynValue.FromColor(new ScriptColor(-operand.Data.ColorValue.R, -operand.Data.ColorValue.G, -operand.Data.ColorValue.B, -operand.Data.ColorValue.A)),
            _ => throw new InvalidOperationException($"Unary minus not supported for {targetType}")
        };
    }
}