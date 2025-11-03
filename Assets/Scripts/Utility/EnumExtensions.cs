using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

public static class EnumExtensions
{
    public static int ToInt<TEnum>(this TEnum InValue) where TEnum : Enum
    {
        Type underlyingType = Enum.GetUnderlyingType(typeof(TEnum));
        if (underlyingType == typeof(uint) || underlyingType == typeof(int))
            return Unsafe.As<TEnum, int>(ref InValue);
        return Convert.ToInt32(InValue);
    }

    public static TEnum ToEnum<TEnum>(this int InValue) where TEnum : Enum
    {
        Type underlyingType = Enum.GetUnderlyingType(typeof(TEnum));
        if (underlyingType == typeof(uint) || underlyingType == typeof(int))
        {
            var v = InValue;
            return Unsafe.As<int, TEnum>(ref v);
        }

        return (TEnum)Enum.ToObject(typeof(TEnum), InValue);
    }

    public static Enum ToFlag(this Enum InValue)
    {
        if (InValue != null)
            return EnumUtils.GetEnumValue(InValue.GetType(), 1 << InValue.ToInt());
        return InValue;
    }

    /// <summary>
    /// 다음 enum 값 리턴
    /// </summary>
    public static Enum NextValue(this Enum InValue)
    {
        Array Arr = Enum.GetValues(InValue.GetType());
        int index = Array.IndexOf(Arr, InValue) + 1;
        return (Enum)Arr.GetValue(Mathf.Clamp(index, 0, Arr.Length - 1));
    }

    // @TODO : 성능 개선 필요
    public static IList<Enum> GetFlaggedValues(this Enum InValue)
    {
        return Enum.GetValues(InValue.GetType()).Cast<Enum>().Where(InValue.HasFlag).ToList();
    }

    // @TODO : 성능 개선 필요
    public static IList<T> GetFlaggedValues<T>(this int InValue) where T : Enum
    {
        if (InValue <= 0) return Array.Empty<T>();
        return Enum.GetValues(typeof(T)).Cast<T>().Where(x =>
        {
            var enumValue = x.ToInt();
            var flag = InValue & enumValue;
            return (flag != 0) && (flag == enumValue);
        }).ToList();
    }

//     public static bool HasFlag<T>(this int InValue, T InEnum) where T : Enum
//     {
//         return GetFlaggedValues<T>(InValue).Contains(InEnum);
//     }
//
//     public static bool HasFlagUnsafe<TEnum>(this TEnum lhs, TEnum rhs) where TEnum :
// #if CSHARP_7_3_OR_NEWER
//             unmanaged, Enum
// #else
//             struct
// #endif
//     {
//
//         unsafe
//         {
// #if CSHARP_7_3_OR_NEWER
//             switch (sizeof(TEnum))
//             {
//                 case 1:
//                     return (*(byte*)(&lhs) & *(byte*)(&rhs)) > 0;
//                 case 2:
//                     return (*(ushort*)(&lhs) & *(ushort*)(&rhs)) > 0;
//                 case 4:
//                     return (*(uint*)(&lhs) & *(uint*)(&rhs)) > 0;
//                 case 8:
//                     return (*(ulong*)(&lhs) & *(ulong*)(&rhs)) > 0;
//                 default:
//                     throw new Exception("Size does not match a known Enum backing type.");
//             }
//
// #else
//                 switch (UnsafeUtility.SizeOf<TEnum>())
//                 {
//                     case 1:
//                         {
//                             byte valLhs = 0;
//                             UnsafeUtility.CopyStructureToPtr(ref lhs, &valLhs);
//                             byte valRhs = 0;
//                             UnsafeUtility.CopyStructureToPtr(ref rhs, &valRhs);
//                             return (valLhs & valRhs) > 0;
//                         }
//                     case 2:
//                         {
//                             ushort valLhs = 0;
//                             UnsafeUtility.CopyStructureToPtr(ref lhs, &valLhs);
//                             ushort valRhs = 0;
//                             UnsafeUtility.CopyStructureToPtr(ref rhs, &valRhs);
//                             return (valLhs & valRhs) > 0;
//                         }
//                     case 4:
//                         {
//                             uint valLhs = 0;
//                             UnsafeUtility.CopyStructureToPtr(ref lhs, &valLhs);
//                             uint valRhs = 0;
//                             UnsafeUtility.CopyStructureToPtr(ref rhs, &valRhs);
//                             return (valLhs & valRhs) > 0;
//                         }
//                     case 8:
//                         {
//                             ulong valLhs = 0;
//                             UnsafeUtility.CopyStructureToPtr(ref lhs, &valLhs);
//                             ulong valRhs = 0;
//                             UnsafeUtility.CopyStructureToPtr(ref rhs, &valRhs);
//                             return (valLhs & valRhs) > 0;
//                         }
//                     default:
//                         throw new Exception("Size does not match a known Enum backing type.");
//                 }
// #endif
//         }
//     }

// #if CSHARP_7_3_OR_NEWER
//     [MethodImpl(MethodImplOptions.AggressiveInlining)]
// #else
//     [MethodImpl((MethodImplOptions)256)]
// #endif
//     public static TEnum AddFlag<TEnum>(this TEnum lhs, TEnum rhs) where TEnum :
// #if CSHARP_7_3_OR_NEWER
//                 unmanaged, Enum
// #else
//                 struct
// #endif
//     {
//
//         unsafe
//         {
// #if CSHARP_7_3_OR_NEWER
//             switch (sizeof(TEnum))
//             {
//                 case 1:
//                     {
//                         var r = *(byte*)(&lhs) | *(byte*)(&rhs);
//                         return *(TEnum*)&r;
//                     }
//                 case 2:
//                     {
//                         var r = *(ushort*)(&lhs) | *(ushort*)(&rhs);
//                         return *(TEnum*)&r;
//                     }
//                 case 4:
//                     {
//                         var r = *(uint*)(&lhs) | *(uint*)(&rhs);
//                         return *(TEnum*)&r;
//                     }
//                 case 8:
//                     {
//                         var r = *(ulong*)(&lhs) | *(ulong*)(&rhs);
//                         return *(TEnum*)&r;
//                     }
//                 default:
//                     throw new Exception("Size does not match a known Enum backing type.");
//             }
//
// #else
//                  
//             switch (UnsafeUtility.SizeOf<TEnum>())
//             {
//                 case 1:
//                     {
//                         byte valLhs = 0;
//                         UnsafeUtility.CopyStructureToPtr(ref lhs, &valLhs);
//                         byte valRhs = 0;
//                         UnsafeUtility.CopyStructureToPtr(ref rhs, &valRhs);
//                         var result = (valLhs | valRhs);
//                         void * r = &result;
//                         TEnum o;
//                         UnsafeUtility.CopyPtrToStructure(r, out o);
//                         return o;
//                     }
//                 case 2:
//                     {
//                         ushort valLhs = 0;
//                         UnsafeUtility.CopyStructureToPtr(ref lhs, &valLhs);
//                         ushort valRhs = 0;
//                         UnsafeUtility.CopyStructureToPtr(ref rhs, &valRhs);
//                         var result = (valLhs | valRhs);
//                         void* r = &result;
//                         TEnum o;
//                         UnsafeUtility.CopyPtrToStructure(r, out o);
//                         return o;
//                     }
//                 case 4:
//                     {
//                         uint valLhs = 0;
//                         UnsafeUtility.CopyStructureToPtr(ref lhs, &valLhs);
//                         uint valRhs = 0;
//                         UnsafeUtility.CopyStructureToPtr(ref rhs, &valRhs);
//                         var result = (valLhs | valRhs);
//                         void* r = &result;
//                         TEnum o;
//                         UnsafeUtility.CopyPtrToStructure(r, out o);
//                         return o;
//                     }
//                 case 8:
//                     {
//                         ulong valLhs = 0;
//                         UnsafeUtility.CopyStructureToPtr(ref lhs, &valLhs);
//                         ulong valRhs = 0;
//                         UnsafeUtility.CopyStructureToPtr(ref rhs, &valRhs);
//                         var result = (valLhs | valRhs);
//                         void* r = &result;
//                         TEnum o;
//                         UnsafeUtility.CopyPtrToStructure(r, out o);
//                         return o;
//                     }
//                 default:
//                     throw new Exception("Size does not match a known Enum backing type.");
//             }
// #endif
//         }
//     }


// #if CSHARP_7_3_OR_NEWER
//     [MethodImpl(MethodImplOptions.AggressiveInlining)]
// #else
//     [MethodImpl((MethodImplOptions)256)]
// #endif
//     public static TEnum RemoveFlag<TEnum>(this TEnum lhs, TEnum rhs) where TEnum :
// #if CSHARP_7_3_OR_NEWER
//                 unmanaged, Enum
// #else
//                 struct
// #endif
//     {
//
//         unsafe
//         {
// #if CSHARP_7_3_OR_NEWER
//             switch (sizeof(TEnum))
//             {
//                 case 1:
//                     {
//                         var r = *(byte*)(&lhs) & ~*(byte*)(&rhs);
//                         return *(TEnum*)&r;
//                     }
//                 case 2:
//                     {
//                         var r = *(ushort*)(&lhs) & ~*(ushort*)(&rhs);
//                         return *(TEnum*)&r;
//                     }
//                 case 4:
//                     {
//                         var r = *(uint*)(&lhs) & ~*(uint*)(&rhs);
//                         return *(TEnum*)&r;
//                     }
//                 case 8:
//                     {
//                         var r = *(ulong*)(&lhs) & ~*(ulong*)(&rhs);
//                         return *(TEnum*)&r;
//                     }
//                 default:
//                     throw new Exception("Size does not match a known Enum backing type.");
//             }
//
// #else
//                  
//             switch (UnsafeUtility.SizeOf<TEnum>())
//             {
//                 case 1:
//                     {
//                         byte valLhs = 0;
//                         UnsafeUtility.CopyStructureToPtr(ref lhs, &valLhs);
//                         byte valRhs = 0;
//                         UnsafeUtility.CopyStructureToPtr(ref rhs, &valRhs);
//                         var result = (valLhs & ~valRhs);
//                         void * r = &result;
//                         TEnum o;
//                         UnsafeUtility.CopyPtrToStructure(r, out o);
//                         return o;
//                     }
//                 case 2:
//                     {
//                         ushort valLhs = 0;
//                         UnsafeUtility.CopyStructureToPtr(ref lhs, &valLhs);
//                         ushort valRhs = 0;
//                         UnsafeUtility.CopyStructureToPtr(ref rhs, &valRhs);
//                         var result = (valLhs & ~valRhs);
//                         void* r = &result;
//                         TEnum o;
//                         UnsafeUtility.CopyPtrToStructure(r, out o);
//                         return o;
//                     }
//                 case 4:
//                     {
//                         uint valLhs = 0;
//                         UnsafeUtility.CopyStructureToPtr(ref lhs, &valLhs);
//                         uint valRhs = 0;
//                         UnsafeUtility.CopyStructureToPtr(ref rhs, &valRhs);
//                         var result = (valLhs & ~valRhs);
//                         void* r = &result;
//                         TEnum o;
//                         UnsafeUtility.CopyPtrToStructure(r, out o);
//                         return o;
//                     }
//                 case 8:
//                     {
//                         ulong valLhs = 0;
//                         UnsafeUtility.CopyStructureToPtr(ref lhs, &valLhs);
//                         ulong valRhs = 0;
//                         UnsafeUtility.CopyStructureToPtr(ref rhs, &valRhs);
//                         var result = (valLhs & ~valRhs);
//                         void* r = &result;
//                         TEnum o;
//                         UnsafeUtility.CopyPtrToStructure(r, out o);
//                         return o;
//                     }
//                 default:
//                     throw new Exception("Size does not match a known Enum backing type.");
//             }
// #endif
//         }
//
//     }

#if CSHARP_7_3_OR_NEWER
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    // public static void SetFlag<TEnum>(ref this TEnum lhs, TEnum rhs) where TEnum : unmanaged, Enum
    // {
    //
    //     unsafe
    //     {
    //         fixed (TEnum* lhs1 = &lhs)
    //         {
    //             switch (sizeof(TEnum))
    //             {
    //                 case 1:
    //                     {
    //                         var r = *(byte*)(lhs1) | *(byte*)(&rhs);
    //                         *lhs1 = *(TEnum*)&r;
    //                         return;
    //                     }
    //                 case 2:
    //                     {
    //                         var r = *(ushort*)(lhs1) | *(ushort*)(&rhs);
    //                         *lhs1 = *(TEnum*)&r;
    //                         return;
    //                     }
    //                 case 4:
    //                     {
    //                         var r = *(uint*)(lhs1) | *(uint*)(&rhs);
    //                         *lhs1 = *(TEnum*)&r;
    //                         return;
    //                     }
    //                 case 8:
    //                     {
    //                         var r = *(ulong*)(lhs1) | *(ulong*)(&rhs);
    //                         *lhs1 = *(TEnum*)&r;
    //                         return;
    //                     }
    //                 default:
    //                     throw new Exception("Size does not match a known Enum backing type.");
    //             }
    //         }
    //     }
    // }
    //
    // [MethodImpl(MethodImplOptions.AggressiveInlining)]
    // public static void ClearFlag<TEnum>(this ref TEnum lhs, TEnum rhs) where TEnum : unmanaged, Enum
    // {
    //
    //     unsafe
    //     {
    //         fixed (TEnum* lhs1 = &lhs)
    //         {
    //             switch (sizeof(TEnum))
    //             {
    //                 case 1:
    //                     {
    //                         var r = *(byte*)(lhs1) & ~*(byte*)(&rhs);
    //                         *lhs1 = *(TEnum*)&r;
    //                         return;
    //                     }
    //                 case 2:
    //                     {
    //                         var r = *(ushort*)(lhs1) & ~*(ushort*)(&rhs);
    //                         *lhs1 = *(TEnum*)&r;
    //                         return;
    //                     }
    //                 case 4:
    //                     {
    //                         var r = *(uint*)(lhs1) & ~*(uint*)(&rhs);
    //                         *lhs1 = *(TEnum*)&r;
    //                         return;
    //                     }
    //                 case 8:
    //                     {
    //                         var r = *(ulong*)(lhs1) & ~*(ulong*)(&rhs);
    //                         *lhs1 = *(TEnum*)&r;
    //                         return;
    //                     }
    //                 default:
    //                     throw new Exception("Size does not match a known Enum backing type.");
    //             }
    //         }
    //     }
    // }
#endif

    /// <summary>
    /// TFrom 타입을 TTo 타입으로 변환 (권장)
    /// </summary>
    public static TTo ConvertType<TFrom, TTo>(this TFrom InValue, bool InSkipUnknownFlags = false)
        where TFrom : struct, Enum
        where TTo : struct, Enum
    {
        return EnumUtils.ConvertType<TFrom, TTo>(InValue, InSkipUnknownFlags);
    }

    /// <summary>
    /// TFrom 타입을 TTo 타입으로 변환 (GC 발생)
    /// </summary>
    public static TTo ConvertType<TTo>(this Enum value, bool skip = true) where TTo : struct, Enum
    {
        return EnumUtils.ConvertTypeRouter<TTo>.Convert(value, skip);
    }
}