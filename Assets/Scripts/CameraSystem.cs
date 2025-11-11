using Unity.Cinemachine;
using UnityEngine;

public class CameraSystem : SingletonBase<CameraSystem>
{
    [SerializeField]private CinemachineCamera tpsCamera;
    [SerializeField]private CinemachineCamera fpsCamera;
    public CinemachineCamera useCamera;
    //public CinemachineCamera OppositeCamera => useCamera == tpsCamera ? fpsCamera : tpsCamera;
    
    public override void Init()
    {
        base.Init();
        useCamera = tpsCamera;
        Debug.Log("CameraSystem Initialized");
    }
    
    public void SetActiveCam()
    {
        var targetCam = useCamera;

        if (targetCam == tpsCamera)
        {
            targetCam = fpsCamera;
        }
        else
        {
            targetCam = tpsCamera;
        }

        tpsCamera.Priority.Value = (targetCam == tpsCamera) ? 20 : 10;
        fpsCamera.Priority.Value = (targetCam == fpsCamera) ? 20 : 10;
    
        useCamera = targetCam;
    }
}
