#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;

namespace REIW
{
    public static class GizmoUtility
    {
        /// <summary>
        /// Gizmos만으로 그리는 간단한 화살표(선 + V자 화살촉). 빌드에서도 동작합니다.
        /// </summary>
        public static void DrawArrow(Vector3 origin, Vector3 direction, Color color,
            float length = 1f, float headLength = 0.25f, float headAngleDeg = 20f)
        {
            if (direction.sqrMagnitude < 1e-8f) return;

            Vector3 dir = direction.normalized;
            Vector3 end = origin + dir * length;

            Gizmos.color = color;
            Gizmos.DrawLine(origin, end);

            // 화살촉(두 갈래)
            Quaternion look = Quaternion.LookRotation(dir);
            Vector3 headDirA = look * Quaternion.Euler(0, 180f - headAngleDeg, 0) * Vector3.forward;
            Vector3 headDirB = look * Quaternion.Euler(0, 180f + headAngleDeg, 0) * Vector3.forward;
            Gizmos.DrawLine(end, end + headDirA * headLength);
            Gizmos.DrawLine(end, end + headDirB * headLength);
        }

        /// <summary>
        /// (에디터에서도/실행 빌드에서도) 화살표를 그리고, 에디터일 경우 라벨을 함께 표시합니다.
        /// </summary>
        public static void DrawLabeledArrow(
            Vector3 origin, Vector3 direction, Color color,
            float length = 1f, float headLength = 0.25f, float headAngleDeg = 20f,
            string label = null, float labelOffset = 0.06f, int fontSize = 11, bool alwaysVisible = false)
        {
            DrawArrow(origin, direction, color, length, headLength, headAngleDeg);

#if UNITY_EDITOR
            if (!string.IsNullOrEmpty(label))
            {
                Vector3 head = origin + direction.normalized * (length + labelOffset);
                DrawGizmoLabel(head, label, color, fontSize, alwaysVisible);
            }
#endif
        }

#if UNITY_EDITOR
        /// <summary>에디터(Scene/Game 뷰)에서 굵은 화살표를 그립니다.</summary>
        public static void DrawArrowHandle(Vector3 origin, Vector3 direction, float size, Color color)
        {
            if (direction.sqrMagnitude < 1e-8f) return;
            using (new Handles.DrawingScope(color))
            {
                Handles.ArrowHandleCap(
                    controlID: 0,
                    position: origin,
                    rotation: Quaternion.LookRotation(direction.normalized),
                    size: size,
                    eventType: EventType.Repaint);
            }
        }

        /// <summary>
        /// 에디터 전용: 굵은 화살표 + 라벨을 함께 그립니다.
        /// </summary>
        public static void DrawLabeledArrowHandle(
            Vector3 origin, Vector3 direction, float size, Color color,
            string label, float labelOffset = 0.06f, int fontSize = 11, bool alwaysVisible = false)
        {
            DrawArrowHandle(origin, direction, size, color);
            if (!string.IsNullOrEmpty(label))
            {
                Vector3 head = origin + direction.normalized * (size + labelOffset);
                DrawGizmoLabel(head, label, color, fontSize, alwaysVisible);
            }
        }

        // ===== 내부: 라벨 그리기 도우미 =====

        static GUIStyle _gizmoLabelStyle;
        static Texture2D _labelBgTex;

        static void EnsureLabelStyle(int fontSize)
        {
            if (_gizmoLabelStyle == null)
            {
                _gizmoLabelStyle = new GUIStyle(EditorStyles.boldLabel)
                {
                    alignment = TextAnchor.MiddleCenter,
                    fontSize = fontSize,
                    padding = new RectOffset(4, 4, 2, 2)
                };
            }
            else
            {
                _gizmoLabelStyle.fontSize = fontSize;
            }

            if (_labelBgTex == null)
            {
                _labelBgTex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
                _labelBgTex.SetPixel(0, 0, new Color(0f, 0f, 0f, 0.35f)); // 반투명 배경
                _labelBgTex.Apply();
            }
            _gizmoLabelStyle.normal.background = _labelBgTex;
        }

        /// <summary>
        /// 에디터 전용: 월드 좌표에 라벨을 그립니다. 필요 시 항상 보이도록 zTest를 무시할 수 있습니다.
        /// </summary>
        static void DrawGizmoLabel(Vector3 worldPos, string text, Color textColor, int fontSize, bool alwaysVisible)
        {
            EnsureLabelStyle(fontSize);
            _gizmoLabelStyle.normal.textColor = textColor;

            var oldZTest = Handles.zTest;
            if (alwaysVisible)
                Handles.zTest = UnityEngine.Rendering.CompareFunction.Always;

            // 간단 버전: Handles.Label
            Handles.Label(worldPos, text, _gizmoLabelStyle);

            if (alwaysVisible)
                Handles.zTest = oldZTest;
        }
#endif
    }
}
