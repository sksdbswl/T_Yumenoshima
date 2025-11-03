using UnityEngine;

namespace REIW
{
    // Attach 별로 달라지면 상속 받아서... 개별 컴포넌트로 만들 어야 함.
    public class AttachObject : MonoBehaviour
    {
        public EnumAttachPart AttachPart = EnumAttachPart.Sickle;
        
        public Transform FxTransform = null;
        
    }
    
}
