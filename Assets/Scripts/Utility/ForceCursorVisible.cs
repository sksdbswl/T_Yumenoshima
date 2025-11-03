using System;
using UnityEngine;

namespace REIW
{
    public class ForceCursorVisible : MonoBehaviour
    {
        private void Update()
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
    }
}
