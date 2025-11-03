using System;
using UnityEngine;

namespace REIW
{
    public abstract class SingletonBase<T> : MonoBehaviour where T : class
    {
        public static T Singleton
        {
            get
            {
                return instance.Value;
            }
        }

        public static bool IsCreated => instance.IsValueCreated;

        private static readonly Lazy<T> instance =
            new Lazy<T>(() =>
            {
                T instance = UnityEngine.Object.FindAnyObjectByType(typeof(T)) as T;

                if (instance == null)
                {
                    GameObject obj = new GameObject(typeof(T).ToString());
                    instance = obj.AddComponent(typeof(T)) as T;

#if UNITY_EDITOR
                    if (UnityEditor.EditorApplication.isPlaying)
                    {
                        DontDestroyOnLoad(obj);
                    }
#else
                   DontDestroyOnLoad(obj);
#endif
                }

                return instance;
            });

        public static bool IsApplicationQuit { get; private set; }

        protected virtual void Awake()
        {
            DontDestroyOnLoad(gameObject);
            Init();
        }

        public virtual void Init()
        {
            
        }

        private void OnApplicationQuit()
        {
            IsApplicationQuit = true;
        }
    }
}