using VMRuntime;
using ValueType = VMRuntime.ValueType;

public class ModuloOperator : OperatorBase
{
    // Seuls les entiers sont supportés pour le modulo.
    protected override ValueType GetConversionType(ValueType leftType, ValueType rightType)
    {
        ValueType common = ConversionTable.CommonType(leftType, rightType);
        if (common == ValueType.Int)
            return ValueType.Int;
        // Si le type commun est Float, on peut forcer une conversion en Int pour les deux.
        if (common == ValueType.Float)
            return ValueType.Int; // la conversion Float->Int sera utilisée (troncature)
        throw new InvalidOperationException($"Modulo requires integer operands, got {leftType} and {rightType}");
    }

    protected override DynValue Evaluate(ValueType targetType, in DynValue left, in DynValue right)
    {
        return DynValue.FromInt(left.Data.IntValue % right.Data.IntValue);
    }
}