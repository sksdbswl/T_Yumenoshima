using System;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace DS.Utilities
{
    using Elements;

    public static class DSElementUtility
    {
        public static Button CreateButton(string text, Action onClick = null)
        {
            Button button = new Button(onClick)
            {
                text = text
            };

            return button;
        }

        public static Foldout CreateFoldout(string title, bool collapsed = false)
        {
            Foldout foldout = new Foldout()
            {
                text = title,
                value = !collapsed
            };

            return foldout;
        }

        public static Port CreatePort(this DSNode node, string portName = "", Orientation orientation = Orientation.Horizontal, Direction direction = Direction.Output, Port.Capacity capacity = Port.Capacity.Single)
        {
            Port port = node.InstantiatePort(orientation, direction, capacity, typeof(bool));

            port.portName = portName;

            return port;
        }

        public static TextField CreateTextField(string value = null, string label = null, EventCallback<ChangeEvent<string>> onValueChanged = null)
        {
            TextField textField = new TextField()
            {
                value = value,
                label = label
            };

            if (onValueChanged != null)
            {
                textField.RegisterValueChangedCallback(onValueChanged);
            }

            return textField;
        }

        public static TextField CreateTextArea(string value = null, string label = null, EventCallback<ChangeEvent<string>> onValueChanged = null)
        {
            TextField textArea = CreateTextField(value, label, onValueChanged);

            textArea.multiline = true;

            return textArea;
        }
        
        
        public static ObjectField CreateObjectField<T>(
            string label,
            T value,
            EventCallback<ChangeEvent<UnityEngine.Object>> onValueChanged = null
        ) where T : UnityEngine.Object
        {
            ObjectField field = new ObjectField(label)
            {
                objectType = typeof(T),
                allowSceneObjects = false,
                value = value   // T는 UnityEngine.Object를 상속하므로 OK
            };

            if (onValueChanged != null)
            {
                field.RegisterValueChangedCallback(onValueChanged);
            }

            return field;
        }

        // value 를 생략하고 싶을 때 쓰는 오버로드
        // public static ObjectField CreateObjectField<T>(
        //     string label,
        //     EventCallback<ChangeEvent<UnityEngine.Object>> onValueChanged = null
        // ) where T : UnityEngine.Object
        // {
        //     return CreateObjectField<T>(label, null, onValueChanged);
        // }
    }
}