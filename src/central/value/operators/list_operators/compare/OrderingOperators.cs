// GreaterThanOperator.cs
using VMRuntime;
using ValueType = VMRuntime.ValueType;

public class GreaterThanOperator : OperatorBase
{
    protected override DynValue Evaluate(ValueType targetType, in DynValue left, in DynValue right)
    {
        return targetType switch
        {
            ValueType.Int => DynValue.FromBool(left.Data.IntValue > right.Data.IntValue),
            ValueType.Float => DynValue.FromBool(left.Data.FloatValue > right.Data.FloatValue),
            _ => throw new InvalidOperationException($"'>' not supported for {targetType}")
        };
    }
}

// LessThanOperator.cs
public class LessThanOperator : OperatorBase
{
    protected override DynValue Evaluate(ValueType targetType, in DynValue left, in DynValue right)
    {
        return targetType switch
        {
            ValueType.Int => DynValue.FromBool(left.Data.IntValue < right.Data.IntValue),
            ValueType.Float => DynValue.FromBool(left.Data.FloatValue < right.Data.FloatValue),
            _ => throw new InvalidOperationException($"'<' not supported for {targetType}")
        };
    }
}

// GreaterOrEqualOperator.cs
public class GreaterOrEqualOperator : OperatorBase
{
    protected override DynValue Evaluate(ValueType targetType, in DynValue left, in DynValue right)
    {
        return targetType switch
        {
            ValueType.Int => DynValue.FromBool(left.Data.IntValue >= right.Data.IntValue),
            ValueType.Float => DynValue.FromBool(left.Data.FloatValue >= right.Data.FloatValue),
            _ => throw new InvalidOperationException($"'>=' not supported for {targetType}")
        };
    }
}

// LessOrEqualOperator.cs
public class LessOrEqualOperator : OperatorBase
{
    protected override DynValue Evaluate(ValueType targetType, in DynValue left, in DynValue right)
    {
        return targetType switch
        {
            ValueType.Int => DynValue.FromBool(left.Data.IntValue <= right.Data.IntValue),
            ValueType.Float => DynValue.FromBool(left.Data.FloatValue <= right.Data.FloatValue),
            _ => throw new InvalidOperationException($"'<=' not supported for {targetType}")
        };
    }
}