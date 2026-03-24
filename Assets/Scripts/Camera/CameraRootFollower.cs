// using UnityEngine;
//
// public class CameraRootFollower : MonoBehaviour
// {
//     public Transform target;          // Player Transform
//     public float damping = 5f;        // 따라가는 부드러움
//
//     void LateUpdate()
//     {
//         transform.position = Vector3.Lerp(
//             transform.position + (transform.forward * -0.5f),
//             target.position,
//             1f - Mathf.Exp(-damping * Time.deltaTime)
//         );
//         // rotation 건드리지 않음 → 카메라 고정각도 유지
//     }
// }

using UnityEngine;

public class CameraRootFollower : MonoBehaviour
{
    public Transform target;
    public float damping = 5f;

    // 카메라 고정 오프셋 — 시네머신 Follow Offset과 역할이 겹치면
    // 여기선 Vector3.zero로 두고 시네머신에서만 조정해도 됨
    public Vector3 offset = Vector3.zero;

    void LateUpdate()
    {
        Vector3 targetPos = target.position + offset; // 목표는 항상 고정

        transform.position = Vector3.Lerp(
            transform.position,
            targetPos,
            1f - Mathf.Exp(-damping * Time.deltaTime)
        );
    }
}