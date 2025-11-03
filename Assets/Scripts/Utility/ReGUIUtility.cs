using System;
using System.Collections.Generic;
using UnityEngine;

namespace REIW
{
    public enum ReGuiAnchor
    {
        TopLeft,
        TopRight,
        BottomLeft,
        BottomRight
    }

    public static class ReGUIUtility
    {
        private struct Entry
        {
            public string Text;
            public Color? Color;
            public GUIStyle Style;
            public int Order;
            public ReGuiAnchor Anchor;
        }

        private static readonly List<Entry> _entries = new();
        private static int _lastFrameRendered = -1;

        // 기본 스타일 캐시
        private static GUIStyle _leftStyle;
        private static GUIStyle _rightStyle;

        private static GUIStyle GetLeftStyle()
        {
            if (_leftStyle == null)
            {
                _leftStyle = new GUIStyle(GUI.skin.label)
                {
                    alignment = TextAnchor.UpperLeft,
                    fontSize = 12,
                    wordWrap = false
                };
            }
            return _leftStyle;
        }

        private static GUIStyle GetRightStyle()
        {
            if (_rightStyle == null)
            {
                _rightStyle = new GUIStyle(GUI.skin.label)
                {
                    alignment = TextAnchor.UpperRight,
                    fontSize = 12,
                    wordWrap = false
                };
            }
            return _rightStyle;
        }

        /// <summary>
        /// 현재 프레임에 표시할 라벨을 큐에 적재합니다. OnGUI 에서 호출하지 마세요.
        /// - color: 지정 시 해당 라벨에만 색상 적용
        /// - anchor: 표시할 화면 코너
        /// - order: 같은 코너 안에서 정렬 우선순위(작을수록 위/왼쪽 먼저)
        /// - style: 개별 라벨 전용 GUIStyle (null이면 기본 스타일)
        /// </summary>
        [System.Diagnostics.Conditional("REIW_DEBUG")]
        public static void Label(
            string text,
            Color? color = null,
            ReGuiAnchor anchor = ReGuiAnchor.TopLeft,
            int order = 0,
            GUIStyle style = null)
        {
            // 여러 곳에서 호출되므로 프레임 체크는 Draw에서만 합니다.
            _entries.Add(new Entry
            {
                Text = text ?? string.Empty,
                Color = color,
                Anchor = anchor,
                Order = order,
                Style = style
            });
        }

        /// <summary>
        /// Key:Value 형태의 라벨 헬퍼, OnGUI 에서 호출하지 마세요.
        /// </summary>
        [System.Diagnostics.Conditional("REIW_DEBUG")]
        public static void LabelKV(
            string key, object value,
            Color? color = null,
            ReGuiAnchor anchor = ReGuiAnchor.TopLeft,
            int order = 0,
            GUIStyle style = null)
        {
            Label($"{key}: {value}", color, anchor, order, style);
        }

        [System.Diagnostics.Conditional("REIW_DEBUG")]
        public static void DrawGUI()
        {
            if (_entries.Count == 0)
                return;

            // 코너별로 분리 및 정렬
            var topLeft = new List<Entry>();
            var topRight = new List<Entry>();
            var bottomLeft = new List<Entry>();
            var bottomRight = new List<Entry>();

            foreach (var e in _entries)
            {
                switch (e.Anchor)
                {
                    case ReGuiAnchor.TopLeft: topLeft.Add(e); break;
                    case ReGuiAnchor.TopRight: topRight.Add(e); break;
                    case ReGuiAnchor.BottomLeft: bottomLeft.Add(e); break;
                    case ReGuiAnchor.BottomRight: bottomRight.Add(e); break;
                }
            }

            topLeft.Sort((a, b) => a.Order.CompareTo(b.Order));
            topRight.Sort((a, b) => a.Order.CompareTo(b.Order));
            bottomLeft.Sort((a, b) => a.Order.CompareTo(b.Order));
            bottomRight.Sort((a, b) => a.Order.CompareTo(b.Order));

            const float margin = 10f;
            const float width = 520f;
            const float rowPad = 2f;

            void DrawStack(List<Entry> list, Rect areaRect, bool alignRight, bool fromBottom)
            {
                if (list.Count == 0) return;

                var styleDefault = alignRight ? GetRightStyle() : GetLeftStyle();

                GUILayout.BeginArea(areaRect);
                try
                {
                    if (fromBottom)
                        GUILayout.FlexibleSpace(); // 아래에서 위로 쌓기

                    foreach (var e in list)
                    {
                        var s = e.Style ?? styleDefault;

                        var prevColor = GUI.contentColor;
                        if (e.Color.HasValue) GUI.contentColor = e.Color.Value;

                        GUILayout.Label(e.Text, s);
                        GUILayout.Space(rowPad);

                        if (e.Color.HasValue) GUI.contentColor = prevColor;
                    }
                }
                finally
                {
                    GUILayout.EndArea();
                }
            }

            // 각 코너 렌더 (Layout/Repaint 모두 동일하게 실행)
            DrawStack(topLeft,
                new Rect(margin, margin, width, Screen.height - margin * 2),
                alignRight: false, fromBottom: false);

            DrawStack(topRight,
                new Rect(Screen.width - width - margin, margin, width, Screen.height - margin * 2),
                alignRight: true, fromBottom: false);

            DrawStack(bottomLeft,
                new Rect(margin, margin, width, Screen.height - margin * 2),
                alignRight: false, fromBottom: true);

            DrawStack(bottomRight,
                new Rect(Screen.width - width - margin, margin, width, Screen.height - margin * 2),
                alignRight: true, fromBottom: true);
        }

        public static void Clear()
        {
            // 이 프레임의 엔트리 사용 후 클리어
            _entries.Clear();
        }
    }
}
