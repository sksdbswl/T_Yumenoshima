using UnityEngine;
using UnityEngine.InputSystem;
using Animancer;


[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    Player player;
    
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 5f;
    //[SerializeField] private Transform cameraPivot;

    [Header("Animancer")]
    [SerializeField] private AnimancerComponent animancer;
    [SerializeField] private AnimationClip idleClip;
    [SerializeField] private AnimationClip walkClip;
    [SerializeField] private float fade = 0.15f;

    private Rigidbody rb;
    private Vector2 moveInput;
    private bool isGrounded;

    private void Awake()
    {
        player = GetComponent<Player>();
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
    }

    private void OnEnable()
    {
        player.moveAction.action.Enable();
        player.jumpAction.action.Enable();
        player.interactAction.action.Enable();
        player.cancelAction.action.Enable();
    
        player.moveAction.action.performed += OnMovePerformed;
        player.moveAction.action.canceled  += OnMoveCanceled;
        player.jumpAction.action.performed += OnJumpPerformed;
        player.interactAction.action.performed += player.OnInteractPerformed;
        player.cancelAction.action.performed   += player.OnInteractCanceled;
    }

    private void OnDisable()
    {
        player.moveAction.action.performed -= OnMovePerformed;
        player.moveAction.action.canceled  -= OnMoveCanceled;
        player.jumpAction.action.performed -= OnJumpPerformed;
        player.interactAction.action.performed -= player.OnInteractPerformed;
        player.cancelAction.action.performed   -= player.OnInteractCanceled;

        player.moveAction.action.Disable();
        player.jumpAction.action.Disable();
        player.interactAction.action.Disable();
        player.cancelAction.action.Disable();
    }

    private void OnMovePerformed(InputAction.CallbackContext ctx)
    {
        moveInput = ctx.ReadValue<Vector2>();
    }

    private void OnMoveCanceled(InputAction.CallbackContext ctx)
    {
        moveInput = Vector2.zero;
    }

    private void OnJumpPerformed(InputAction.CallbackContext ctx)
    {
        TryJump();
    }


    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            CameraSystem.Singleton.SetActiveCam();
        }
    }
    
    private void FixedUpdate()
    {
        Vector3 move = new Vector3(moveInput.x, 0, moveInput.y);
        // if (cameraPivot)
        // {
        //     Vector3 camForward = cameraPivot.forward;
        //     camForward.y = 0;
        //     Quaternion camRot = Quaternion.LookRotation(camForward);
        //     move = camRot * move;
        // }

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

        // 애니메이션
        if (move.magnitude > 0.1f)
            animancer.Play(walkClip, fade);
        else
            animancer.Play(idleClip, fade);
    }

    private void TryJump()
    {
        if (isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        isGrounded = true;
    }

    private void OnCollisionExit(Collision collision)
    {
        isGrounded = false;
    }
}



// using REIW;
// using REIW.Animations.Character;
// using UnityEngine;
//
//
// [RequireComponent(typeof(CharacterController))]
// public class PlayerMovement : MonoBehaviour
// {
//     [SerializeField] private CharacterAnimation characterAnimation;
//     [Header("Move")]
//     [SerializeField] private float walkSpeed = 2.0f;
//     [SerializeField] private float runSpeed  = 5.5f;
//     [SerializeField] private float acceleration = 12f;
//     [SerializeField] private float rotationSpeed = 720f;
//     [Header("Jump/Gravity")]
//     [SerializeField] private float jumpHeight = 1.6f;
//     [SerializeField] private float gravity    = -19.6f;
//     [SerializeField] private float groundedStick = -2f;
//
//     private CharacterController _cc;
//     private CharacterAnimationMovement Movement => characterAnimation?.Movement;
//
//     private Vector3 _vel;
//     private float _currSpeed;
//
//     private void Awake()
//     {
//         _cc = GetComponent<CharacterController>();
//     }
//
//     private void Update()
//     {
//         if (Movement == null) return;
//
//         // 이동 속도 결정 (브리지가 ActionInputType을 채워줌)
//         float targetSpeed = 0f;
//         if (Movement.CurrentActionInputType == eCharacterActionInputType.RUN)
//             targetSpeed = runSpeed;
//         else if (Movement.CurrentActionInputType == eCharacterActionInputType.WALK)
//             targetSpeed = walkSpeed;
//
//         _currSpeed = Mathf.MoveTowards(_currSpeed, targetSpeed, acceleration * Time.deltaTime);
//
//         // 입력 방향 (카메라 기준 원하면 Camera.main 기준으로 변환)
//         Vector3 inputDir = new Vector3(Movement.RawMoveInput.x, 0f, Movement.RawMoveInput.y);
//         if (inputDir.sqrMagnitude > 0.0001f)
//         {
//             var rot = Quaternion.LookRotation(inputDir);
//             transform.rotation = Quaternion.RotateTowards(transform.rotation, rot, rotationSpeed * Time.deltaTime);
//         }
//
//         // 점프/중력
//         if (_cc.isGrounded)
//         {
//             if (_vel.y < 0f) _vel.y = groundedStick;
//
//             if (Movement.RequestJump)
//             {
//                 _vel.y = Mathf.Sqrt(Mathf.Abs(gravity) * 2f * Mathf.Max(0.01f, jumpHeight));
//                 Movement.RequestJump = false; // 소비
//             }
//         }
//         _vel.y += gravity * Time.deltaTime;
//
//         // 최종 이동
//         Vector3 horizontal = transform.forward * _currSpeed;
//         _cc.Move((horizontal + new Vector3(0, _vel.y, 0)) * Time.deltaTime);
//
//         // 애니메 파라미터(있다면): 속도/수직속도 갱신
//         Movement.ForwardSpeed = _currSpeed;     // 프로젝트 규약에 맞게 필드/프로퍼티 사용
//         Movement.VerticalSpeed = _vel.y;
//     }
// }
//
