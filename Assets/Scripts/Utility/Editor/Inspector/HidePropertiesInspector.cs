using System.Collections.Generic;
using UnityEditor;

namespace REIW
{
    public class HidePropertiesInspector : Editor
    {
        protected List<string> _hideProperties = new();
        private string[] _drawExcludeProperties;

        protected virtual void Awake()
        {
            _hideProperties.Add("m_Script");
        }

        protected virtual void OnEnable()
        {
            _drawExcludeProperties = _hideProperties.ToArray();
        }

        protected virtual void PreDrawProperties()
        {
        }

        protected virtual void PostDrawProperties()
        {
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            PreDrawProperties();

            DrawPropertiesExcluding(serializedObject, _drawExcludeProperties);

            PostDrawProperties();

            serializedObject.ApplyModifiedProperties();
        }
    }
}
