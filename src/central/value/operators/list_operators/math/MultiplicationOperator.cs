using VMRuntime;
using ValueType = VMRuntime.ValueType;

public class MultiplicationOperator : OperatorBase
{
    protected override DynValue Evaluate(ValueType targetType, in DynValue left, in DynValue right)
    {
        return targetType switch
        {
            ValueType.Int => DynValue.FromInt(left.Data.IntValue * right.Data.IntValue),
            ValueType.Float => DynValue.FromFloat(left.Data.FloatValue * right.Data.FloatValue),
            ValueType.Vector2 => DynValue.FromVector2(left.Data.Vector2Value * right.Data.Vector2Value),
            ValueType.Vector2i => DynValue.FromVector2i(left.Data.Vector2iValue * right.Data.Vector2iValue),
            ValueType.Vector3 => DynValue.FromVector3(left.Data.Vector3Value * right.Data.Vector3Value),
            ValueType.Vector3i => DynValue.FromVector3i(left.Data.Vector3iValue * right.Data.Vector3iValue),
            ValueType.Color => DynValue.FromColor(left.Data.ColorValue * right.Data.ColorValue),
            _ => throw new InvalidOperationException($"Multiplication not supported for {targetType}")
        };
    }
}