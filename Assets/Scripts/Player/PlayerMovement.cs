using UnityEngine;
using UnityEngine.InputSystem;
using Animancer;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    private Player player;
    private Rigidbody rb;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 5f;

    [Header("Animancer")]
    [SerializeField] private AnimancerComponent animancer;

    [Header("Animancer Idle")]
    [SerializeField] private AnimationClip idleClip;
    [SerializeField] private AnimationClip equipIdleClip;

    [Header("Animancer Walk")]
    [SerializeField] private AnimationClip walkClip;
    [SerializeField] private AnimationClip equipWalkClip;

    [Header("Animancer Equip")]
    [SerializeField] private AnimationClip equipWearClip;
    [SerializeField] private AnimationClip equipDiscodClip;

    [Header("Animancer Gather")]
    [SerializeField] private AnimationClip gatherTreeClip;   // 나무 베기
    [SerializeField] private AnimationClip gatherShakeClip;  // 나무 흔들기
    [SerializeField] private AnimationClip gatherDigClip;    // 모래 파기
    [SerializeField] private AnimationClip gatherPickupClip; // 아이템 줍기

    [Header("Animancer Fishing")]
    [SerializeField] private AnimationClip fishingStartClip;
    [SerializeField] private AnimationClip fishingIdleClip;
    [SerializeField] private AnimationClip fishingPullClip;
    [SerializeField] private AnimationClip fishingGatherClip;
    [SerializeField] private AnimationClip fishingMissClip;
    [SerializeField] private AnimationClip fishingGetClip;
    [SerializeField] private AnimationClip fishingHoldClip;

    [Header("Animancer Hold")]
    [SerializeField] private AnimationClip holdClip;

    [Header("Animancer Bee")]
    [SerializeField] private AnimationClip meetBeeClip;

    [SerializeField] private float fade = 0.15f;

    // 땅에 붙어있는지
    private bool isGrounded = true;

    // ---- 상태 플래그들 ----
    [Header("Player State")]
    private bool isFishing = false;
    private bool isGathering = false;
    private float gatherTimer = 0f;                // gather 중 3초 카운트용

    // 이동 애니를 막기 위한 편의 프로퍼티
    private bool IsBusy => isFishing || isGathering;

    private void Awake()
    {
        player = GetComponent<Player>();
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
    }

    private void OnEnable()
    {
        player.inputHandler.OnJump += TryJump;
    }

    private void OnDisable()
    {
        player.inputHandler.OnJump -= TryJump;
    }

    private void Update()
    {
        // // 장비 장착/해제 
        // if (Input.GetKeyDown(KeyCode.Alpha1))
        // {
        //     // equip 장착
        // }
        //
        // if (Input.GetKeyDown(KeyCode.Alpha2))
        // {
        //     // equip 해제
        // }
        //
        // // --- 채집 시작 (테스트용) ---
        // if (Input.GetKeyDown(KeyCode.Alpha3))
        // {
        //     if (!IsBusy)
        //         StartGather();
        // }
        //
        // // --- 낚시 시작 (테스트용) ---
        // if (Input.GetKeyDown(KeyCode.Alpha4))
        // {
        //     if (!IsBusy)
        //         PlayFishingStart();
        // }
        //
        // // 채집 타이머 처리 (3초)
        // if (isGathering)
        // {
        //     gatherTimer -= Time.deltaTime;
        //     if (gatherTimer <= 0f)
        //     {
        //         EndGather(); // 3초 지나면 채집 상태 종료
        //     }
        // }
    }

    private void FixedUpdate()
    {
        // 이동 처리
        Vector2 moveInput = player.inputHandler.MoveInput;
        Vector3 move = new Vector3(moveInput.x, 0, moveInput.y);

        Vector3 targetVel = move * moveSpeed;
        Vector3 vel = rb.linearVelocity;
        vel.x = targetVel.x;
        vel.z = targetVel.z;
        rb.linearVelocity = vel;

        // 회전
        if (move.sqrMagnitude > 0.01f)
        {
            Quaternion targetRot = Quaternion.LookRotation(move);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, 12f * Time.fixedDeltaTime);
        }

        // 액션(낚시, 채집 등) 중에는 Idle/Walk 애니를 건드리지 않음
        if (IsBusy)
            return;

        // 이동 애니메이션
        if (move.magnitude > 0.1f)
            animancer.Play(walkClip, fade);
        else
            animancer.Play(idleClip, fade);
    }

    // ----------------------------
    // 점프 (지금은 애니 없이 물리만)
    // ----------------------------
    private void TryJump()
    {
        if (!isGrounded) return;

        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }

    private void OnCollisionEnter(Collision collision)
    {
        isGrounded = true;
    }

    private void OnCollisionStay(Collision collision)
    {
        isGrounded = true;
    }

    private void OnCollisionExit(Collision collision)
    {
        isGrounded = false;
    }

    // ============================
    //        채집 (Gather)
    // ============================
    private void StartGather()
    {
        isGathering = true;
        gatherTimer = 3f; // 채집 중 3초 타이머 시작

        // 예시: 나무 베기 → 흔들기 → 들기
        var state = animancer.Play(gatherTreeClip, fade);

        if (state.Events(this, out var e))
        {
            e.OnEnd = () =>
            {
                var midState = animancer.Play(gatherShakeClip, fade);

                if (midState.Events(this, out var midEvents))
                {
                    midEvents.OnEnd = () =>
                    {
                        animancer.Play(holdClip, fade);
                        // hold 애니가 재생 중이어도,
                        // 타이머 3초가 끝나면 EndGather()에서 isGathering을 false로 만든다.
                    };
                }
            };
        }
    }

    private void EndGather()
    {
        isGathering = false;
        gatherTimer = 0f;
        // 여기서 따로 애니를 바꾸지 않으면
        // holdClip이 계속 재생되다가,
        // 플레이어가 다시 움직이면 FixedUpdate에서 Idle/Walk로 알아서 넘어감.
    }

    // ============================
    //        낚시 (Fishing)
    //  순수 애니 순차 테스트용
    // ============================
    private void PlayFishingStart()
    {
        isFishing = true;

        var state = animancer.Play(fishingStartClip, fade);
        if (state.Events(this, out var e))
            e.OnEnd = PlayFishingIdle;
    }

    private void PlayFishingIdle()
    {
        var state = animancer.Play(fishingIdleClip, fade);
        if (state.Events(this, out var e))
            e.OnEnd = PlayFishingPull;
    }

    private void PlayFishingPull()
    {
        var state = animancer.Play(fishingPullClip, fade);
        if (state.Events(this, out var e))
            e.OnEnd = PlayFishingGather;
    }

    private void PlayFishingGather()
    {
        var state = animancer.Play(fishingGatherClip, fade);
        if (state.Events(this, out var e))
            e.OnEnd = PlayFishingGet;
    }

    private void PlayFishingGet()
    {
        var state = animancer.Play(fishingGetClip, fade);
        if (state.Events(this, out var e))
            e.OnEnd = PlayFishingHold;
    }

    private void PlayFishingHold()
    {
        var state = animancer.Play(fishingHoldClip, fade);
        if (state.Events(this, out var e))
            e.OnEnd = PlayFishingMiss;
    }

    private void PlayFishingMiss()
    {
        animancer.Play(fishingMissClip, fade);
        isFishing = false;   // 낚시 시퀀스 끝 → 다시 Idle/Walk 허용
    }
}
