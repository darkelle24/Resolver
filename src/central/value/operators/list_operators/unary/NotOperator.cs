using VMRuntime;
using ValueType = VMRuntime.ValueType;

public class NotOperator : UnaryOperator
{
    protected override ValueType GetConversionType(ValueType operandType)
    {
        // Force la conversion vers Bool
        return ValueType.Bool;
    }

    protected override DynValue Evaluate(ValueType targetType, in DynValue operand)
    {
        return DynValue.FromBool(!operand.AsBool());
    }
}