using VMRuntime;
using ValueType = VMRuntime.ValueType;

public class EqualOperator : OperatorBase
{
    protected override DynValue Evaluate(ValueType targetType, in DynValue left, in DynValue right)
    {
        // left et right sont déjà convertis au même type (targetType)
        return DynValue.FromBool(left.Equals(right));
    }
}