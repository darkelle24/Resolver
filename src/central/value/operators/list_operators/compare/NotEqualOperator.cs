using VMRuntime;
using ValueType = VMRuntime.ValueType;

public class NotEqualOperator : OperatorBase
{
    protected override DynValue Evaluate(ValueType targetType, in DynValue left, in DynValue right)
    {
        return DynValue.FromBool(!left.Equals(right));
    }
}