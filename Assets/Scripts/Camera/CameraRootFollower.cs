using UnityEngine;

public class CameraRootFollower : MonoBehaviour
{
    public Transform target;          // Player Transform
    public float damping = 5f;        // 따라가는 부드러움

    void LateUpdate()
    {
        transform.position = Vector3.Lerp(
            transform.position + (transform.forward * -0.5f),
            target.position,
            1f - Mathf.Exp(-damping * Time.deltaTime)
        );
        // rotation 건드리지 않음 → 카메라 고정각도 유지
    }
}