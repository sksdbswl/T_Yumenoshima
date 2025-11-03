// using UnityEditor;
// using UnityEngine;
// using System.Reflection;
//
// namespace REIW.Animations
// {
//     using REIW;
//     
//     [CustomPropertyDrawer(typeof(AnimationEventDataSO.DataInfo))]
//     public class AnimationEventInfoDrawer : PropertyDrawer
//     {
//         public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
//         {
//             EditorGUI.BeginProperty(position, label, property);
//             var indent = EditorGUI.indentLevel;
//             EditorGUI.indentLevel = 0;
//
//             var eventTypeProp = property.FindPropertyRelative("_eventType");
//             var clipProp = property.FindPropertyRelative("_eventClip");
//             var normalProp = property.FindPropertyRelative("_eventNormalTime");
//             var animationCurveProp = property.FindPropertyRelative("_animationCurve");
//
//             var cameraFovProp = property.FindPropertyRelative("_cameraFov");
//             var cameraSpeedProp = property.FindPropertyRelative("_cameraSpeed");
//
//             var cameranoiseProfile = property.FindPropertyRelative("_noiseProfile");
//             var cameranoiseAmplitude = property.FindPropertyRelative("_amplitude");
//             var cameranoiseFrequency = property.FindPropertyRelative("_frequency");
//             var cameranoiseTime = property.FindPropertyRelative("_time");
//
//             var customeventname = property.FindPropertyRelative("_customEventName");
//             var customeventparameters = property.FindPropertyRelative("_customEventParameters");
//
//             var cameraMoveFrom = property.FindPropertyRelative("_cameramoveFrom");
//             var cameraMoveTo = property.FindPropertyRelative("_cameramoveTo");
//             var cameraRotationFrom = property.FindPropertyRelative("_camerarotFrom");
//             var cameraRotationTo = property.FindPropertyRelative("_camerarotTo");
//             
//             var endGrappleState = property.FindPropertyRelative("_endGrappleState");
//             var endGrappleOffsetTime = property.FindPropertyRelative("_endGrappleOffsetTime");
//
//             float lineHeight = EditorGUIUtility.singleLineHeight;
//             float spacing = 2f;
//             float y = position.y;
//
//             var eventType = (AnimationEventDataSO.eAnimationEventType)eventTypeProp.enumValueIndex;
//
//             DrawProperty(eventTypeProp, eventType == AnimationEventDataSO.eAnimationEventType.None ? Color.red : Color.white);
//
//             if (eventType == AnimationEventDataSO.eAnimationEventType.None)
//                 return;
//             
//             DrawProperty(clipProp);
//             DrawProperty(normalProp);
//
//             y += lineHeight + spacing;
//
//             var grapplestate = (CharacterMoveGrapple.GrappleState)endGrappleState.enumValueIndex;
//             
//             switch (eventType)
//             {
//                 case AnimationEventDataSO.eAnimationEventType.Custom:
//                     DrawProperty(customeventname);
//                     DrawProperty(customeventparameters);
//                     break;
//
//                 case AnimationEventDataSO.eAnimationEventType.Camera_FOV:
//                     DrawProperty(animationCurveProp);
//                     DrawProperty(cameraFovProp);
//                     DrawProperty(cameraSpeedProp);
//                     break;
//
//                 case AnimationEventDataSO.eAnimationEventType.Camera_FOV_RESET:
//                     DrawProperty(animationCurveProp);
//                     DrawProperty(cameraSpeedProp);
//                     break;
//
//                 case AnimationEventDataSO.eAnimationEventType.Camera_Shake:
//                     DrawProperty(cameranoiseProfile);
//                     DrawProperty(cameranoiseAmplitude);
//                     DrawProperty(cameranoiseFrequency);
//                     DrawProperty(cameranoiseTime);
//                     break;
//
//                 case AnimationEventDataSO.eAnimationEventType.Camera_Move:
//                     DrawProperty(animationCurveProp);
//                     DrawProperty(cameraSpeedProp);
//                     DrawProperty(cameraMoveFrom);
//                     DrawProperty(cameraMoveTo);
//                     break;
//
//                 case AnimationEventDataSO.eAnimationEventType.Camera_Move_Grapple:
//                     DrawProperty(animationCurveProp);
//                     DrawProperty(cameraSpeedProp);
//                     DrawProperty(cameraMoveFrom);
//                     DrawProperty(cameraMoveTo);
//                     DrawProperty(endGrappleState, grapplestate == CharacterMoveGrapple.GrappleState.None ? Color.red : Color.white);
//                     DrawProperty(endGrappleOffsetTime);
//                     break;
//
//                 case AnimationEventDataSO.eAnimationEventType.Camera_Rotate:
//                     DrawProperty(animationCurveProp);
//                     DrawProperty(cameraSpeedProp);
//                     DrawProperty(cameraRotationFrom);
//                     DrawProperty(cameraRotationTo);
//                     break;
//                 
//                 case AnimationEventDataSO.eAnimationEventType.Camera_Rotate_Grapple:
//                     DrawProperty(animationCurveProp);
//                     DrawProperty(cameraSpeedProp);
//                     DrawProperty(cameraRotationFrom);
//                     DrawProperty(cameraRotationTo);
//                     DrawProperty(endGrappleState, grapplestate == CharacterMoveGrapple.GrappleState.None ? Color.red : Color.white);
//                     DrawProperty(endGrappleOffsetTime);
//                     break;
//             }
//
//             EditorGUI.EndProperty();
//
//             void DrawProperty(SerializedProperty property, Color? color =  null)
//             {
//                 var oldColor = GUI.contentColor;
//                 GUI.contentColor = color.HasValue ? color.Value : Color.white; 
//                 
//                 if (IsRangeProperty(property, out float min, out float max))
//                     EditorGUI.Slider(new Rect(position.x, y, position.width, lineHeight), property, min, max);
//                 else
//                     EditorGUI.PropertyField(new Rect(position.x, y, position.width, lineHeight), property);
//                 
//                 y += lineHeight + spacing;
//                 GUI.contentColor = oldColor;
//             }
//             
//             bool IsRangeProperty(SerializedProperty property, out float min, out float max)
//             {
//                 min = 0;
//                 max = 0;
//                 
//                 var targetObj = property.serializedObject.targetObject;
//                 var field = targetObj.GetType().GetField(
//                     property.name,
//                     BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
//                 
//                 if (field == null)
//                     return false;
//
//                 var attrs =  field.GetCustomAttributes(typeof(RangeAttribute), true);
//                 if ((attrs?.Length ?? -1) <= 0)
//                     return false;
//                 
//                 RangeAttribute  range = (RangeAttribute)attrs[0];
//                 min = range.min;
//                 max = range.max;
//                 return true;
//             }
//         }
//
//         public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
//         {
//             var eventTypeProp = property.FindPropertyRelative("_eventType");
//
//             int lineCount = 4;
//
//             var eventType = (AnimationEventDataSO.eAnimationEventType)eventTypeProp.enumValueIndex;
//
//             lineCount += eventType switch
//             {
//                 AnimationEventDataSO.eAnimationEventType.None             => -2,
//                 AnimationEventDataSO.eAnimationEventType.Camera_FOV       => 3,
//                 AnimationEventDataSO.eAnimationEventType.Camera_FOV_RESET => 2,
//                 AnimationEventDataSO.eAnimationEventType.Camera_Shake     => 4,
//                 AnimationEventDataSO.eAnimationEventType.Custom           => 2,
//                 AnimationEventDataSO.eAnimationEventType.Camera_Move or
//                     AnimationEventDataSO.eAnimationEventType.Camera_Rotate => 4,
//                 AnimationEventDataSO.eAnimationEventType.Camera_Move_Grapple or
//                     AnimationEventDataSO.eAnimationEventType.Camera_Rotate_Grapple => 6,
//                 _ => 0,
//             };
//
//             float lineHeight = EditorGUIUtility.singleLineHeight + 2f;
//             return lineCount * lineHeight;
//         }
//     }
// }