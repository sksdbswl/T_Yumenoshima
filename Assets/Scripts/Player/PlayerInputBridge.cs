// using REIW;
// using REIW.Animations.Character;
// using UnityEngine;
// using UnityEngine.InputSystem;
//
//
// // eCharacterActionInputType은 프로젝트에 이미 있다고 가정 (NONE/WALK/RUN/JUMP 등)
// // CharacterAnimation -> Movement 접근이 이미 있는 구조를 활용
// [RequireComponent(typeof(PlayerInput))]
// public class CharacterInputBridge : MonoBehaviour
// {
//     [Header("REQUIRED")]
//     [SerializeField] private CharacterAnimation characterAnimation; // 기존 Animation 루트
//     [SerializeField] private PlayerInput playerInput;
//
//     [Header("Speed Gates")]
//     [SerializeField] private float walkDeadzone = 0.05f; // 입력 잡음 제거
//     [SerializeField] private bool  useRunHold   = true;  // Shift 등 눌렀을 때만 달리기
//
//     private InputAction _move;
//     private InputAction _jump;
//     private InputAction _runHold;
//
//     private CharacterAnimationMovement Movement => characterAnimation?.Movement;
//
//     private void Awake()
//     {
//         if (!playerInput) playerInput = GetComponent<PlayerInput>();
//     }
//
//     private void OnEnable()
//     {
//         _move    = playerInput.actions["Move"];       // Value(Vector2)
//         _jump    = playerInput.actions["Jump"];       // Button
//         _runHold = playerInput.actions.FindAction("RunHold"); // Button (선택)
//
//         if (_jump != null)
//             _jump.performed += OnJumpPerformed;
//     }
//
//     private void OnDisable()
//     {
//         if (_jump != null)
//             _jump.performed -= OnJumpPerformed;
//     }
//
//     private void Update()
//     {
//         if (Movement == null) return;
//
//         // 1) 이동 입력 읽기
//         Vector2 move = _move != null ? _move.ReadValue<Vector2>() : Vector2.zero;
//         bool hasMove = move.sqrMagnitude > (walkDeadzone * walkDeadzone);
//
//         // 2) 달리기 홀드 여부
//         bool runPressed = _runHold != null && _runHold.IsPressed();
//
//         // 3) Movement에 입력 전달 (방향/원시값은 프로젝트 규약에 맞게)
//         Movement.MovementDirection = move; // 존재한다고 가정(없다면 Movement에 맞는 필드/프로퍼티로 교체)
//
//         // 4) 상태머신이 읽어가는 ActionInputType 설정
//         if (!hasMove)
//         {
//             Movement.CurrentActionInputType = eCharacterActionInputType.NONE;
//         }
//         else
//         {
//             if (useRunHold && runPressed)
//                 Movement.CurrentActionInputType = eCharacterActionInputType.RUN;
//             else
//                 Movement.CurrentActionInputType = eCharacterActionInputType.WALK;
//         }
//
//         // 점프는 OnJumpPerformed에서 JUMP로 단발 세팅 → 상태 전이 후 NONE으로 되돌리기(한 프레임)
//         if (Movement.CurrentActionInputType == eCharacterActionInputType.JUMP)
//         {
//             // 상태머신이 NextStateType에서 JUMP로 전이하도록 한 프레임 유지 후 NONE으로 리셋
//             Movement.CurrentActionInputType = eCharacterActionInputType.NONE;
//         }
//     }
//
//     private void OnJumpPerformed(InputAction.CallbackContext ctx)
//     {
//         if (Movement == null) return;
//
//         // 공중에서 점프 입력 무시 등 조건이 Movement에 있다면 여기서 체크 가능
//         if (Movement.IsGrounded) // Movement에 존재한다고 가정
//         {
//             Movement.CurrentActionInputType = eCharacterActionInputType.JUMP;
//             Movement.RequestJump = true; // 물리/속도 갱신 계층에서 처리하도록 플래그 (존재 시)
//         }
//     }
// }
//
