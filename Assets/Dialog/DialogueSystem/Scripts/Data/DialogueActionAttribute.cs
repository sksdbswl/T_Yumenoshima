using System;

namespace Dialog.DialogueSystem.Scripts.Data
{
    // Attribute: 아무 함수나 다 노출하면 위험/노이즈라, 노출할 함수에만 태그
    [AttributeUsage(AttributeTargets.Method)]
    public class DialogueActionAttribute : Attribute
    {
        
    }
}