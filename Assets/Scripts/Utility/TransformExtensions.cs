using UnityEngine;
using System.Collections.Generic;

namespace REIW
{
    public static class TransformExtensions
    {
        public static int GetSiblingIndexOfHighest(this Transform parent, RectTransform[] list)
        {
            int highest = -1;
            foreach (var rect in list)
            {
                if (rect != null && rect.transform.parent == parent)
                    highest = Mathf.Max(highest, rect.GetSiblingIndex());
            }
            return highest;
        }

        public static int GetSiblingIndexOfLowest(this Transform parent, RectTransform[] list)
        {
            int lowest = int.MaxValue;
            bool found = false;
            foreach (var rect in list)
            {
                if (rect != null && rect.transform.parent == parent)
                {
                    lowest = Mathf.Min(lowest, rect.GetSiblingIndex());
                    found = true;
                }
            }
            return found ? lowest : -1;
        }
        
        public static Transform FindAllChild(this Transform trans, string name)
        {
            return FindAllChild(trans, (x) => string.Equals(x.name, name, System.StringComparison.OrdinalIgnoreCase));
        }
        
        public static Transform FindAllChild(this Transform trans, System.Func<Transform, bool> func)
        {
            if (trans == null)
                return null;

            if (func(trans))
                return trans;

            for (int i = 0; i < trans.childCount; ++i)
            {
                Transform transform = trans.GetChild(i);
                Transform findtrans = FindAllChild(transform, func);
                if (findtrans != null)
                    return findtrans;
            }

            return null;
        }

        public static List<Transform> FindListAllChild(this Transform trans, System.Func<Transform, bool> conditionfunc = null)
        {
            var list = new List<Transform>();
            if (trans == null) 
                return list;

            list.Add(trans);
            
            var stack = new Stack<Transform>();

            for (int i = 0; i < trans.childCount; ++i)
                stack.Push(trans.GetChild(i));

            while (stack.Count > 0)
            {
                var cur = stack.Pop();

                if (conditionfunc == null || conditionfunc(cur))
                    list.Add(cur);

                // 자식들 순회
                for (int i = 0; i < cur.childCount; ++i)
                    stack.Push(cur.GetChild(i));
            }

            return list;
        }

        public static bool IsEmptyObject(this Transform trans)
        {
            return (trans.GetComponents<Component>().Length == 1);
        }

        public static void ResetTransform(this Transform trans, Transform parent, Transform dest)
        {
            trans.SetParent(parent, false);

            trans.name = dest.name;
            trans.localRotation = dest.localRotation;
            trans.localPosition = dest.localPosition;
            trans.localScale = dest.localScale;
        }
        
        public static void ResetTransform(this Transform trans, Transform parent, Quaternion rotation, Vector3 pos, Vector3 scale)
        {
            trans.SetParent(parent, false);

            trans.localRotation = rotation;
            trans.localPosition = pos;
            trans.localScale = scale;
        }

    }
}
