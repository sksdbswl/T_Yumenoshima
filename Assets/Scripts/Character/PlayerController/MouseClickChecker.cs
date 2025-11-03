using UnityEngine;
using System;

namespace REIW
{
    [Serializable]
    public class MouseClickChecker
    {
        public enum MouseButton
        {
            Left,
            Right,
        }
        public enum MouseAction
        {
            Up,
            Down,
        }
        [Serializable]
        private struct ButtonTimes
        {
            public float DownTime;
            public float UpTime;
            public int UpFrame;
            
            public float LastClickTime;
            public int DoubleClickUpFrame;
        }
        
        [SerializeField] private float _clickMaxDuration = 0.2f;      // 클릭 간격
        [SerializeField] private float _doubleClickMaxGap = 0.4f;    // 더블클릭 간격

        private ButtonTimes _left;
        private ButtonTimes _right;
        
        public void OnMouse(MouseButton button, MouseAction action)
        {
            ref ButtonTimes buttonTimes = ref Get(button);

            if (action == MouseAction.Down)
            {
                buttonTimes.DownTime = Time.time;
                return;
            }

            // MouseAction.Up
            buttonTimes.UpTime = Time.time;
            buttonTimes.UpFrame = Time.frameCount;    
            
            if (IsClickButton(in buttonTimes))
            {
                bool isDouble = (buttonTimes.UpTime - buttonTimes.LastClickTime) <= _doubleClickMaxGap;
                if (isDouble)
                    buttonTimes.DoubleClickUpFrame = Time.frameCount;

                buttonTimes.LastClickTime = buttonTimes.UpTime;
            }
        }


        private ref ButtonTimes Get(MouseButton button) => ref button == MouseButton.Left ? ref _left : ref _right;

        public bool LeftClicked => IsClickButton(_left);
        public bool RightClicked => IsClickButton(_right);
        public bool LeftDoubleClicked => _left.DoubleClickUpFrame == Time.frameCount;
        public bool RightDoubleClicked => _right.DoubleClickUpFrame == Time.frameCount;
        
        private bool IsClickButton(in ButtonTimes t)
        {
            if (t.UpFrame != Time.frameCount) 
                return false;

            float duration = t.UpTime - t.DownTime;
            return duration >= 0 && duration <= _clickMaxDuration;
        }
    }
}
