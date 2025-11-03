using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using System.Reflection;
using REIW;
using UnityEngine;
#if UNITY_2019_1_OR_NEWER
using Unity.Collections.LowLevel.Unsafe;
#endif
using System.Linq;

public static class EnumUtils
{
    public static T ParsingStringToEnumType<T>(string InValue, bool InIgnoreCase = false) where T : struct
    {
        if (string.IsNullOrEmpty(InValue) == false)
        {
            if (InIgnoreCase)
            {
                if (Enum.TryParse(InValue, InIgnoreCase, out T enumType))
                    return enumType;
            }
            else if (Enum.IsDefined(typeof(T), InValue))
            {
                return (T)Enum.Parse(typeof(T), InValue);
            }
        }

        return default;
    }

    /// <summary>
    /// enum 이름으로 enum type 얻기
    /// </summary>
    /// <remarks>
    /// @CAUTION : Reflection 사용한다. 성능 부담 주의할 것.
    /// </remarks>
    public static Type GetEnumType(string InEnumName)
    {
        if (string.IsNullOrEmpty(InEnumName) == false)
        {
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                var type = assembly.GetType(InEnumName);
                if (type == null)
                    continue;
                if (type.IsEnum)
                    return type;
            }
        }

        return null;
    }

    /// <summary>
    /// enum 이름과 enum 값 string으로 enum value 얻기
    /// </summary>
    /// <remarks>
    /// @CAUTION : 내부적으로 Reflection 사용한다. 성능 부담 주의할 것.
    /// </remarks>
    public static Enum GetEnumValue(string InEnumName, string InEnumValue = "", bool InIsGetDefault = true)
    {
        return GetEnumValue(GetEnumType(InEnumName), InEnumValue, InIsGetDefault);
    }

    /// <summary>
    /// enum 타입과 enum 값 string으로 enum value 얻기
    /// </summary>
    public static Enum GetEnumValue(Type InEnumType, string InEnumValue = "", bool InIsGetDefault = true)
    {
        if (InEnumType != null)
        {
            string[] names = Enum.GetNames(InEnumType);
            if (!names.IsNullOrEmpty())
            {
                if (string.IsNullOrEmpty(InEnumValue) == false)
                {
                    for (int i = 0; i < names.Length ; ++i)
                    {
                        if (names[i] == InEnumValue)
                            return (Enum)Enum.GetValues(InEnumType).GetValue(i);
                    }
                }

                return InIsGetDefault ? (Enum)Enum.GetValues(InEnumType).GetValue(0) : null;
            }
        }

        return null;
    }

    /// <summary>
    /// enum 이름과 enum 인덱스로 enum value 얻기
    /// </summary>
    /// <remarks>
    /// @CAUTION : 내부적으로 Reflection 사용한다. 성능 부담 주의할 것.
    /// </remarks>
    public static Enum GetEnumValueByIndex(string InEnumName, int InEnumIndex)
    {
        return GetEnumValueByIndex(GetEnumType(InEnumName), InEnumIndex);
    }

    /// <summary>
    /// enum 타입과 enum 인덱스로 enum value 얻기
    /// </summary>
    public static Enum GetEnumValueByIndex(Type InEnumType, int InEnumIndex)
    {
        if (InEnumType != null)
        {
            Array values = Enum.GetValues(InEnumType);
            if (values != null && values.Length > 0)
                return (Enum)values.GetValue(Mathf.Max(0, InEnumIndex));
        }

        return null;
    }

    /// <summary>
    /// enum 이름과 enum 값으로 enum value 얻기
    /// </summary>
    public static Enum GetEnumValue(string InEnumName, int InEnumValue)
    {
        return GetEnumValue(GetEnumType(InEnumName), InEnumValue);
    }

    /// <summary>
    /// enum 이름과 enum 값으로 enum value 얻기
    /// </summary>
    public static Enum GetEnumValue(Type InEnumType, int InEnumValue)
    {
        return (Enum)Enum.ToObject(InEnumType, InEnumValue);
    }

    /// <summary>
    /// enum 타입 리스트 얻기
    /// </summary>
    public static ReadOnlyCollection<Type> GetEnumTypeList(Type InEnumType)
    {
        if (InEnumType != null)
        {
            Type[] types = InEnumType.GetNestedTypes(BindingFlags.Public | BindingFlags.NonPublic);
            if (types.Length > 0)
            {
                List<Type> typeList = new List<Type>();
                for (int i = 0; i < types.Length; ++i)
                {
                    Type type = types[i];
                    if (type.IsEnum)
                        typeList.Add(type);
                }

                if (typeList.Count > 0)
                    return typeList.AsReadOnly();
            }
        }

        return null;
    }

    public static int PackFlag(int value, int bitDigits)
    {
        int flagBit = 1 << bitDigits;
        int flagValue = value & (flagBit - 1);
        flagValue |= flagBit;
        return flagValue;
    }

    public static TEnum PackFlag<TEnum>(TEnum value, int bitDigits) where TEnum : Enum
    {
        return PackFlag(value.ToInt(), bitDigits).ToEnum<TEnum>();
    }

    public static (int value, bool flag) UnpackFlag(int value, int bitDigits)
    {
        int flagBit = 1 << bitDigits;
        return (value & (flagBit - 1), (value & flagBit) != 0);
    }

    public static (TEnum value, bool flag) UnpackFlag<TEnum>(TEnum value, int bitDigits) where TEnum : Enum
    {
        var values = UnpackFlag(value.ToInt(), bitDigits);
        return (values.value.ToEnum<TEnum>(), values.flag);
    }

    public static int GetUnpackValue(int value, int bitDigits)
    {
        int flagBit = 1 << bitDigits;
        return value & (flagBit - 1);
    }

    public static TEnum GetUnpackValue<TEnum>(TEnum value, int bitDigits) where TEnum : Enum
    {
        return GetUnpackValue(value.ToInt(), bitDigits).ToEnum<TEnum>();
    }

    public static bool GetUnpackFlag(int value, int bitDigits)
    {
        int flagBit = 1 << bitDigits;
        return (value & flagBit) != 0;
    }

    public static bool GetUnpackFlag<TEnum>(TEnum value, int bitDigits) where TEnum : Enum
    {
        return GetUnpackFlag(value.ToInt(), bitDigits);
    }

    public static bool IsPackedFlag(int value, int bitDigits)
    {
        return  (value & ~((1 << bitDigits) - 1)) != 0;
    }

    public static bool IsPackedFlag<TEnum>(TEnum value, int bitDigits) where TEnum : Enum
    {
        return IsPackedFlag(value.ToInt(), bitDigits);
    }
    
#region Enum Name Mapper
    /// <summary>
    /// TFrom 타입을 TTo 타입으로 변환
    /// </summary>
    public static TTo ConvertType<TFrom, TTo>(TFrom InValue, bool InSkipUnknownFlags = false)
        where TFrom : struct, Enum
        where TTo : struct, Enum
    {
        var C = Cache<TFrom, TTo>.Instance; // 캐시 강제 생성
        ulong v = ToULong(InValue);

        // Flags ↔ Flags
        if (C.FromFlags && C.ToFlags)
        {
            ulong acc = 0UL;
            var pairs = C.FlagPairs;
            for (int i = 0; i < pairs.Length; i++)
                if ((v & pairs[i].fromBit) != 0) acc |= pairs[i].toBit;

            if (!InSkipUnknownFlags)
            {
                ulong unknown = v & ~C.FromAllBits;
                if (unknown != 0UL)
                    return default;
            }

            return FromULong<TTo>(acc, C.ToTypeCode);
        }

        if (C.SingleMap.TryGetValue(v, out ulong mapped))
            return FromULong<TTo>(mapped, C.ToTypeCode);

        return default;
    }

    /// <summary>앱 시작 시 한번 호출해 캐시 워밍업(권장)</summary>
    public static void Warm<TFrom, TTo>()
        where TFrom : struct, Enum
        where TTo   : struct, Enum
    {
        _ = Cache<TFrom, TTo>.Instance; // 정적 초기화 트리거
    }

    private static ulong ToULong<T>(T e) where T : Enum
    {
#if UNITY_2019_1_OR_NEWER
        // 박싱 없이 비트 재해석
        return UnsafeUtility.As<T, ulong>(ref e);
#else
        // Fallback(박싱 가능): 최초 캐시 빌드 1회만 사용됨
        return Convert.ToUInt64(e);
#endif
    }

    private static TTo FromULong<TTo>(ulong bits, TypeCode toTypeCode) where TTo : struct, Enum
    {
#if UNITY_2019_1_OR_NEWER
        // 박싱 없이 대상 enum으로 비트 캐스팅
        switch (toTypeCode)
        {
            case TypeCode.SByte:  { sbyte  v = unchecked((sbyte) bits);  return UnsafeUtility.As<sbyte,  TTo>(ref v); }
            case TypeCode.Byte:   { byte   v = unchecked((byte)  bits);  return UnsafeUtility.As<byte,   TTo>(ref v); }
            case TypeCode.Int16:  { short  v = unchecked((short) bits);  return UnsafeUtility.As<short,  TTo>(ref v); }
            case TypeCode.UInt16: { ushort v = unchecked((ushort)bits);  return UnsafeUtility.As<ushort, TTo>(ref v); }
            case TypeCode.Int32:  { int    v = unchecked((int)   bits);  return UnsafeUtility.As<int,    TTo>(ref v); }
            case TypeCode.UInt32: { uint   v = unchecked((uint)  bits);  return UnsafeUtility.As<uint,   TTo>(ref v); }
            case TypeCode.Int64:  { long   v = unchecked((long)  bits);  return UnsafeUtility.As<long,   TTo>(ref v); }
            default:              { ulong  v = bits;                      return UnsafeUtility.As<ulong,  TTo>(ref v); }
        }
#else
        // Fallback(박싱): 캐시 없을 때만 사용되도록 설계(호출 빈도 낮음)
        return (TTo)Enum.ToObject(typeof(TTo), bits);
#endif
    }

    private static bool IsPow2(ulong x) => x != 0 && (x & (x - 1)) == 0;

    private sealed class Cache<TFrom, TTo>
        where TFrom : struct, Enum
        where TTo   : struct, Enum
    {
        public static readonly Cache<TFrom, TTo> Instance = new();

        public readonly bool FromFlags;
        public readonly bool ToFlags;
        public readonly TypeCode ToTypeCode;
        public readonly Dictionary<ulong, ulong> SingleMap; // fromValue → toValue
        public readonly (ulong fromBit, ulong toBit)[] FlagPairs;
        public readonly ulong FromAllBits;

        private Cache()
        {
            var fromType = typeof(TFrom);
            var toType   = typeof(TTo);

            FromFlags = Attribute.IsDefined(fromType, typeof(FlagsAttribute));
            ToFlags   = Attribute.IsDefined(toType,   typeof(FlagsAttribute));
            ToTypeCode = Type.GetTypeCode(Enum.GetUnderlyingType(toType));

            // to: 이름 → 값(비트) 사전 (대소문자 무시)
            var toNames = Enum.GetNames(toType);
            var toVals  = (TTo[])Enum.GetValues(toType);
            var toByName = new Dictionary<string, ulong>(toNames.Length, StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < toNames.Length; i++)
                toByName[toNames[i]] = ToULong(toVals[i]);

            // 단일 이름 매핑(from 값 → to 값)
            var fromNames = Enum.GetNames(fromType);
            var fromVals  = (TFrom[])Enum.GetValues(fromType);
            SingleMap = new Dictionary<ulong, ulong>(fromVals.Length);
            for (int i = 0; i < fromNames.Length; i++)
            {
                string name = fromNames[i];
                ulong  fv   = ToULong(fromVals[i]);
                if (toByName.TryGetValue(name, out ulong tv))
                    SingleMap[fv] = tv;
            }

            // Flags 전용: 단일 비트들만 추려서 1:1 매핑 테이블 구성
            if (FromFlags && ToFlags)
            {
                var pairs = new List<(ulong fromBit, ulong toBit)>(fromVals.Length);
                ulong all = 0UL;

                for (int i = 0; i < fromNames.Length; i++)
                {
                    ulong fv = ToULong(fromVals[i]);
                    if (!IsPow2(fv)) continue;     // 단일 비트만 대상
                    if (fv == 0) continue;         // 0은 제외

                    string name = fromNames[i];
                    if (toByName.TryGetValue(name, out ulong tv))
                    {
                        pairs.Add((fv, tv));
                        all |= fv;
                    }
                }

                FlagPairs   = pairs.ToArray();
                FromAllBits = all;
            }
            else
            {
                FlagPairs   = Array.Empty<(ulong, ulong)>();
                FromAllBits = 0UL;
            }
        }
    }

    public static class ConvertTypeRouter<TTo> where TTo : struct, Enum
    {
        static readonly Dictionary<Type, Func<Enum, bool, TTo>> Cache = new();

        public static TTo Convert(Enum value, bool skip)
        {
            var ft = value.GetType();
            if (!Cache.TryGetValue(ft, out var fn))
            {
                fn = Build(ft);                  // fromType별 1회 빌드
                Cache[ft] = fn;
            }
            return fn(value, skip);
        }

        private static Func<Enum, bool, TTo> Build(Type fromType)
        {
            var mi = typeof(EnumUtils).GetMethod(nameof(ConvertType)).MakeGenericMethod(fromType, typeof(TTo));
            var v = Expression.Parameter(typeof(Enum), "InValue");
            var s = Expression.Parameter(typeof(bool), "InSkipUnknownFlags");
            var call = Expression.Call(mi, Expression.Convert(v, fromType), s);
            return Expression.Lambda<Func<Enum, bool, TTo>>(call, v, s).Compile();
        }
    }
#endregion

    public static T[] GetEnumValues<T>(T max = default) where T : Enum
    {
        var fields = typeof(T)
            .GetFields(BindingFlags.Public | BindingFlags.Static)
            .OrderBy(f => f.MetadataToken) // 선언 순
            .ToArray();

        var list = new List<T>(fields.Length);
        bool hasLimit = !EqualityComparer<T>.Default.Equals(max, default);
                
        foreach (var f in fields)
        {
            var val = (T)f.GetValue(null)!;
            if (hasLimit && EqualityComparer<T>.Default.Equals(val, max))
                break;
            
            list.Add(val);
        }
                    
        return list.ToArray();
    }
}
