using UnityEngine;

namespace REIW
{
    public interface IInputActionValidator
    {
        bool CanExecute(eStaminaActionType actionType);
    }
}
