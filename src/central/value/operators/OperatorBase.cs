// OperatorBase.cs
using System;

namespace VMRuntime;

public abstract class OperatorBase
{
    /// <summary>
    /// Détermine le type cible vers lequel les deux opérandes seront convertis.
    /// Par défaut : type commun calculé via la table de promotion.
    /// </summary>
    protected virtual ValueType GetConversionType(ValueType leftType, ValueType rightType)
    {
        return ConversionTable.CommonType(leftType, rightType);
    }

    /// <summary>
    /// Exécute l'opération sur deux opérandes déjà convertis au type cible.
    /// </summary>
    protected abstract DynValue Evaluate(ValueType targetType, in DynValue left, in DynValue right);

    public DynValue Resolve(in DynValue left, in DynValue right)
    {
        // 1. Déterminer le type vers lequel convertir (peut être surchargé)
        ValueType targetType = GetConversionType(left.Type, right.Type);

        // 2. Convertir les opérandes
        if (!ConversionTable.TryConvert(left, targetType, out DynValue lConv))
            throw new VmValueCastException(targetType.ToString(), left, GetType().Name);
        if (!ConversionTable.TryConvert(right, targetType, out DynValue rConv))
            throw new VmValueCastException(targetType.ToString(), right, GetType().Name);

        // 3. Appeler l'implémentation concrète
        return Evaluate(targetType, in lConv, in rConv);
    }
}

public abstract class UnaryOperator
{
    /// <summary>
    /// Détermine le type cible vers lequel l'opérande sera converti.
    /// Par défaut : pas de conversion (type identique à l'opérande).
    /// </summary>
    protected virtual ValueType GetConversionType(ValueType operandType)
    {
        return operandType;
    }

    /// <summary>
    /// Exécute l'opération sur l'opérande (éventuellement converti).
    /// </summary>
    protected abstract DynValue Evaluate(ValueType targetType, in DynValue operand);

    public DynValue Resolve(in DynValue operand)
    {
        ValueType targetType = GetConversionType(operand.Type);

        if (!ConversionTable.TryConvert(operand, targetType, out DynValue converted))
        {
            throw new VmValueCastException(targetType.ToString(), operand, GetType().Name);
        }

        return Evaluate(targetType, in converted);
    }
}
