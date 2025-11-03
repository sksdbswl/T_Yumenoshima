// using System;
// using Animancer;
// using UnityEngine;
//
// namespace REIW.Animations
// {
//     public static class AnimancerExtensions
//     {
//         public static ExitEvent SetExitEvent(this AnimancerNode InNode, Action InCallback)
//         {
//             ExitEvent exitEvent = new(InNode, InCallback);
//             exitEvent.Enable();
//             return exitEvent;
//         }
//
//         public static AnimancerState GetCurrentChildState(this ManualMixerState InMixerState)
//         {
//             for (int i = 0; i < InMixerState.ChildCount; ++i)
//             {
//                 var child = InMixerState.GetChild(i);
//                 if (child.IsCurrent)
//                     return child;
//             }
//
//             return null;
//         }
//
//         public static AnimancerState GetCurrentChildState(this SequenceState InSequenceState)
//         {
//             return InSequenceState.GetChild(InSequenceState.GetActiveChildIndex(InSequenceState.RawTime));
//         }
//
//         public static AnimancerState GetNextChildState(this SequenceState InSequenceState)
//         {
//             var index = InSequenceState.GetActiveChildIndex(InSequenceState.RawTime) + 1;
//             return index < InSequenceState.ChildCount ? InSequenceState.GetChild(index) : null;
//         }
//
//         public static ITransition GetTransition(this AnimancerState InState)
//         {
//             object key = InState.Key;
//             while (key is AnimancerState s)
//                 key = s.Key;
//             return key as ITransition;
//         }
//     }
// }
