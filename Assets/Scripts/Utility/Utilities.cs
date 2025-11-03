using System;
using System.Collections;
using System.Collections.Generic;
using Object = UnityEngine.Object;

namespace REIW
{
    public static class Utilities
    {
        public static bool IsNullOrDestroyed(this object obj)
        {
            return obj == null || (obj is Object unityObject && unityObject == null);
        }

        public static bool IsNullOrEmpty(this IEnumerable source)
        {
            if (source is null)
                return true;

            var e = source.GetEnumerator();
            try
            {
                return !e.MoveNext();
            }
            finally
            {
                (e as IDisposable)?.Dispose();
            }
        }

        public static bool IsNullOrEmpty<T>(this IEnumerable<T> source)
        {
            if (source is null) return true;
            if (source is ICollection<T> c) return c.Count == 0;
            if (source is IReadOnlyCollection<T> rc) return rc.Count == 0;

            using var e = source.GetEnumerator();
            return !e.MoveNext();
        }

        public static int BitsForValue(ulong value, bool zeroIsZeroBits = false)
        {
            if (value == 0)
                return zeroIsZeroBits ? 0 : 1;

            int bits = 0;
            while (value != 0)
            {
                bits++;
                value >>= 1;
            }

            return bits;
        }
        
        public static void DeepCopy(object src, object tar)
        {
            System.Type srctype = src.GetType();
            System.Type destype = tar.GetType();
            foreach (System.Reflection.FieldInfo finfo in srctype.GetFields(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance))
            {
                System.Reflection.FieldInfo copyinfo = destype.GetField(finfo.Name, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                if (copyinfo != null)
                {
                    copyinfo.SetValue(tar, finfo.GetValue(src));
                }
            }
        }
    }
}
