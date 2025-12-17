// using System;
// using R3;
// using UnityEngine;
//
//
// public class CameraRegisterHelper : MonoBehaviour
// {
//     public void Start()
//     {
//         var thisCamera = GetComponent<Camera>();
//         // UIManager.Singleton.UICamera = thisCamera;
//             
//         // 기존 CullingMask에서 UI 레이어 제외
//         int uiLayer = LayerMask.NameToLayer("UI");
//         if (uiLayer >= 0)
//         {
//             thisCamera.cullingMask &= ~(1 << uiLayer);
//         }
//             
//         var d = Disposable.CreateBuilder();
//         Observable.EveryUpdate()
//             .Select(_ => Camera.main)
//             .Where(mainCam => mainCam)
//             .Take(TimeSpan.FromSeconds(1))
//             .Subscribe(mainCam =>
//             {
//                 // UICamera Stack Register To MainCamera - Stack 
//                 UnityEngine.Rendering.Universal.UniversalAdditionalCameraData mainUacd
//                     = mainCam.GetComponent<UnityEngine.Rendering.Universal.UniversalAdditionalCameraData>();
//             
//                 if (!mainUacd.cameraStack.Contains(UIManager.Singleton.UICamera))
//                 {
//                     mainUacd.cameraStack.Add(UIManager.Singleton.UICamera);
//                 }
//             })
//             .AddTo(ref d);
//             
//         Observable.EveryUpdate()
//             .Select(_ => new
//             {
//                 mainCam = Camera.main,
//                 inputController = InputController.Singleton
//             })
//             .Where(x => x.mainCam != null && x.inputController != null && x.inputController.PlayerInput != null)
//             .Take(TimeSpan.FromSeconds(1))
//             .Subscribe(x =>
//             {
//                 // MainCamera Register To PlayerInput - Camera 
//                 x.inputController.PlayerInput.camera = x.mainCam;
//             })
//             .AddTo(ref d);
//             
//         d.RegisterTo(destroyCancellationToken);
//     }
// }