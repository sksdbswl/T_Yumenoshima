using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerAnimationStateMachine : AnimationStateMachine
{
    private CharacterController _controller;

    [Header("Speed Thresholds")]
    public float walkSpeedThreshold = 0.1f;
    public float runSpeedThreshold  = 4.0f;

    [Header("Input")]
    public KeyCode runKey = KeyCode.LeftShift;

    protected override void Awake()
    {
        base.Awake();
        _controller = GetComponent<CharacterController>();
    }

    private void Update()
    {
        UpdateStateFromInput();
    }

    private void UpdateStateFromInput()
    {
        Vector3 horizontalVel = _controller.velocity;
        horizontalVel.y = 0f;
        float speed = horizontalVel.magnitude;

        bool isRunKey = Input.GetKey(runKey);

        if (speed < walkSpeedThreshold)
        {
            SetState(AnimState.Idle);
        }
        else
        {
            if (isRunKey && speed > runSpeedThreshold)
                SetState(AnimState.Run);
            else
                SetState(AnimState.Walk);
        }

        // 여기서 앞으로 Jump, Attack 등 플레이어 전용 상태 추가 가능
        // ex) if (Input.GetButtonDown("Fire1")) -> Attack 상태로 전환 등
    }
}