// ConversionTable.cs
using System;


namespace VMRuntime;

public delegate DynValue ValueConverterFn(in DynValue value);

public static class ConversionTable
{
    // Table de conversion : pour chaque (source, cible), une fonction de conversion ou null
    private static readonly ValueConverterFn[,] _converterTable;

    // Table de type commun : pour chaque couple (A, B), le type le plus "large"
    private static readonly ValueType[,] _commonType;

    static ConversionTable()
    {
        int typeCount = Enum.GetValues<ValueType>().Length;
        _converterTable = new ValueConverterFn[typeCount, typeCount];
        _commonType = new ValueType[typeCount, typeCount];

        // Initialiser la table de type commun avec des valeurs par défaut (gauche prioritaire)
        for (int i = 0; i < typeCount; i++)
            for (int j = 0; j < typeCount; j++)
                _commonType[i, j] = (ValueType)i; // sera écrasé par les règles

        // ---------- CONVERSIONS PRIMITIVES ----------
        RegisterConversion(ValueType.Int, ValueType.Float,
            (in DynValue v) => DynValue.FromFloat(v.Data.IntValue));
        RegisterConversion(ValueType.Float, ValueType.Int,
            (in DynValue v) => DynValue.FromInt((int)v.Data.FloatValue));
        RegisterConversion(ValueType.Bool, ValueType.Int,
            (in DynValue v) => DynValue.FromInt(v.Data.BoolValue ? 1 : 0));
        RegisterConversion(ValueType.Int, ValueType.Bool,
            (in DynValue v) => DynValue.FromBool(v.Data.IntValue != 0));
        RegisterConversion(ValueType.Bool, ValueType.Float,
            (in DynValue v) => DynValue.FromFloat(v.Data.BoolValue ? 1f : 0f));
        RegisterConversion(ValueType.Float, ValueType.Bool,
            (in DynValue v) => DynValue.FromBool(v.Data.FloatValue != 0f));

        // ---------- VECTOR CONVERSIONS ----------
        RegisterConversion(ValueType.Vector2, ValueType.Vector2i,
            (in DynValue v) => DynValue.FromVector2i(new ScriptVector2I((int)v.Data.Vector2Value.X, (int)v.Data.Vector2Value.Y)));
        RegisterConversion(ValueType.Vector2i, ValueType.Vector2,
            (in DynValue v) => DynValue.FromVector2(new ScriptVector2(v.Data.Vector2iValue.X, v.Data.Vector2iValue.Y)));
        RegisterConversion(ValueType.Vector3, ValueType.Vector3i,
            (in DynValue v) => DynValue.FromVector3i(new ScriptVector3I((int)v.Data.Vector3Value.X, (int)v.Data.Vector3Value.Y, (int)v.Data.Vector3Value.Z)));
        RegisterConversion(ValueType.Vector3i, ValueType.Vector3,
            (in DynValue v) => DynValue.FromVector3(new ScriptVector3(v.Data.Vector3iValue.X, v.Data.Vector3iValue.Y, v.Data.Vector3iValue.Z)));

        // ---------- COLOR <-> VECTOR3 ----------
        RegisterConversion(ValueType.Color, ValueType.Vector3,
            (in DynValue v) =>
            {
                var c = v.Data.ColorValue;
                return DynValue.FromVector3(new ScriptVector3(c.R, c.G, c.B));
            });
        RegisterConversion(ValueType.Vector3, ValueType.Color,
            (in DynValue v) =>
            {
                var vec = v.Data.Vector3Value;
                return DynValue.FromColor(new ScriptColor(vec.X, vec.Y, vec.Z, 1f));
            });

        // ---------- SCALAR -> VECTOR (broadcast) ----------
        RegisterConversion(ValueType.Int, ValueType.Vector2,
            (in DynValue v) => DynValue.FromVector2(new ScriptVector2(v.Data.IntValue, v.Data.IntValue)));
        RegisterConversion(ValueType.Int, ValueType.Vector2i,
            (in DynValue v) => DynValue.FromVector2i(new ScriptVector2I(v.Data.IntValue, v.Data.IntValue)));
        RegisterConversion(ValueType.Int, ValueType.Vector3,
            (in DynValue v) => DynValue.FromVector3(new ScriptVector3(v.Data.IntValue, v.Data.IntValue, v.Data.IntValue)));
        RegisterConversion(ValueType.Int, ValueType.Vector3i,
            (in DynValue v) => DynValue.FromVector3i(new ScriptVector3I(v.Data.IntValue, v.Data.IntValue, v.Data.IntValue)));
        RegisterConversion(ValueType.Float, ValueType.Vector2,
            (in DynValue v) => DynValue.FromVector2(new ScriptVector2(v.Data.FloatValue, v.Data.FloatValue)));
        RegisterConversion(ValueType.Float, ValueType.Vector2i,
            (in DynValue v) => { int c = (int)v.Data.FloatValue; return DynValue.FromVector2i(new ScriptVector2I(c, c)); });
        RegisterConversion(ValueType.Float, ValueType.Vector3,
            (in DynValue v) => DynValue.FromVector3(new ScriptVector3(v.Data.FloatValue, v.Data.FloatValue, v.Data.FloatValue)));
        RegisterConversion(ValueType.Float, ValueType.Vector3i,
            (in DynValue v) => { int c = (int)v.Data.FloatValue; return DynValue.FromVector3i(new ScriptVector3I(c, c, c)); });

        // ---------- SCALAR -> COLOR (broadcast) ----------
        RegisterConversion(ValueType.Int, ValueType.Color,
            (in DynValue v) => DynValue.FromColor(new ScriptColor(v.Data.IntValue, v.Data.IntValue, v.Data.IntValue, 1f)));
        RegisterConversion(ValueType.Float, ValueType.Color,
            (in DynValue v) => DynValue.FromColor(new ScriptColor(v.Data.FloatValue, v.Data.FloatValue, v.Data.FloatValue, 1f)));

        // ---------- TO STRING (implicite) ----------
        RegisterConversion(ValueType.Int, ValueType.String, (in DynValue v) => DynValue.FromString(v.ToString() ?? ""));
        RegisterConversion(ValueType.Float, ValueType.String, (in DynValue v) => DynValue.FromString(v.ToString() ?? ""));
        RegisterConversion(ValueType.Bool, ValueType.String, (in DynValue v) => DynValue.FromString(v.ToString() ?? ""));
        RegisterConversion(ValueType.Vector2, ValueType.String, (in DynValue v) => DynValue.FromString(v.ToString() ?? ""));
        RegisterConversion(ValueType.Vector2i, ValueType.String, (in DynValue v) => DynValue.FromString(v.ToString() ?? ""));
        RegisterConversion(ValueType.Vector3, ValueType.String, (in DynValue v) => DynValue.FromString(v.ToString() ?? ""));
        RegisterConversion(ValueType.Vector3i, ValueType.String, (in DynValue v) => DynValue.FromString(v.ToString() ?? ""));
        RegisterConversion(ValueType.Color, ValueType.String, (in DynValue v) => DynValue.FromString(v.ToString() ?? ""));
        RegisterConversion(ValueType.Array, ValueType.String, (in DynValue v) => DynValue.FromString(v.ToString() ?? ""));
        RegisterConversion(ValueType.Dictionary, ValueType.String, (in DynValue v) => DynValue.FromString(v.ToString() ?? ""));

        // ---------- TABLE DE PROMOTION (CommonType) ----------
        // Règles : le type le plus "large" l'emporte.
        SetCommon(ValueType.Int, ValueType.Float, ValueType.Float);
        SetCommon(ValueType.Int, ValueType.Bool, ValueType.Int);     // Bool promu en Int
        SetCommon(ValueType.Float, ValueType.Bool, ValueType.Float);
        // Scalaires -> Vecteurs
        SetCommon(ValueType.Int, ValueType.Vector2, ValueType.Vector2);
        SetCommon(ValueType.Float, ValueType.Vector2, ValueType.Vector2);
        SetCommon(ValueType.Int, ValueType.Vector2i, ValueType.Vector2i);
        SetCommon(ValueType.Float, ValueType.Vector2i, ValueType.Vector2i);
        SetCommon(ValueType.Int, ValueType.Vector3, ValueType.Vector3);
        SetCommon(ValueType.Float, ValueType.Vector3, ValueType.Vector3);
        SetCommon(ValueType.Int, ValueType.Vector3i, ValueType.Vector3i);
        SetCommon(ValueType.Float, ValueType.Vector3i, ValueType.Vector3i);
        // Vector2 / Vector2i (Vector2 domine car il a des composantes float)
        SetCommon(ValueType.Vector2, ValueType.Vector2i, ValueType.Vector2);
        // Vector3 / Vector3i (Vector3 domine)
        SetCommon(ValueType.Vector3, ValueType.Vector3i, ValueType.Vector3);
        // Color vs Vector3 : Color domine (contient Vector3 + alpha)
        SetCommon(ValueType.Color, ValueType.Vector3, ValueType.Color);
        // Bonus : String ne gagne jamais contre un autre type
        for (int i = 0; i < typeCount; i++)
        {
            if ((ValueType)i == ValueType.String) continue;
            _commonType[i, (int)ValueType.String] = (ValueType)i;       // gauche non-String
            _commonType[(int)ValueType.String, i] = (ValueType)i;       // droite non-String
        }
        // String + String = String
        _commonType[(int)ValueType.String, (int)ValueType.String] = ValueType.String;

        // Dans le constructeur statique de ConversionTable, après les autres règles

        // Array ne se mélange qu'avec Array
        for (int i = 0; i < typeCount; i++)
        {
            if ((ValueType)i == ValueType.Array) continue;
            _commonType[i, (int)ValueType.Array] = ValueType.Array;      // l'opération échouera car pas de conversion
            _commonType[(int)ValueType.Array, i] = ValueType.Array;
        }
        _commonType[(int)ValueType.Array, (int)ValueType.Array] = ValueType.Array;

        // Dictionary ne se mélange qu'avec Dictionary
        for (int i = 0; i < typeCount; i++)
        {
            if ((ValueType)i == ValueType.Dictionary) continue;
            _commonType[i, (int)ValueType.Dictionary] = ValueType.Dictionary;
            _commonType[(int)ValueType.Dictionary, i] = ValueType.Dictionary;
        }
        _commonType[(int)ValueType.Dictionary, (int)ValueType.Dictionary] = ValueType.Dictionary;
    }

    // Enregistre une conversion
    private static void RegisterConversion(ValueType from, ValueType to, ValueConverterFn converter)
    {
        _converterTable[(int)from, (int)to] = converter;
    }

    // Enregistre une règle de promotion symétrique
    private static void SetCommon(ValueType a, ValueType b, ValueType result)
    {
        _commonType[(int)a, (int)b] = result;
        _commonType[(int)b, (int)a] = result;
    }

    /// <summary>Tente de convertir une valeur vers le type cible. Retourne false si impossible.</summary>
    public static bool TryConvert(in DynValue value, ValueType targetType, out DynValue result)
    {
        if (value.Type == targetType)
        {
            result = value;
            return true;
        }

        var converter = _converterTable[(int)value.Type, (int)targetType];
        if (converter == null)
        {
            result = DynValue.Null;
            return false;
        }

        result = converter(value);
        return true;
    }

    /// <summary>Renvoie le type commun le plus "large" entre deux types, selon les règles de promotion.</summary>
    public static ValueType CommonType(ValueType a, ValueType b)
    {
        return _commonType[(int)a, (int)b];
    }
}