using UnityEngine;
using System;

namespace REIW
{
    public class CacheMonoBehaviour : MonoBehaviour
    {
        private Transform _myTransform = null;
        public Transform MyTransform => _myTransform ??= this.transform;

        private RectTransform _myRectTransform = null;
        public RectTransform MyRectTransform => _myRectTransform ??= MyTransform as RectTransform; 

        private GameObject _myGameObject = null;
        public GameObject MyGameObject => _myGameObject ??= this.gameObject;

        protected bool IsApplicationQuit
        {
            get;
            private set;
        } = false;
        
        void OnApplicationQuit()
        {
            IsApplicationQuit = true;
        }
    }
}
