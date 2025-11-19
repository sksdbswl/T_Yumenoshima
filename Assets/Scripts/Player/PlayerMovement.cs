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
    private bool isGrounded;

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
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            CameraSystem.Singleton.SetActiveCam();
        }
    }
    
    private void FixedUpdate()
    {
        // 여기서 inputHandler의 MoveInput을 읽어옴
        Vector2 moveInput = player.inputHandler.MoveInput;

        Vector3 move = new Vector3(moveInput.x, 0, moveInput.y);

        Vector3 targetVel = move * moveSpeed;
        Vector3 vel = rb.linearVelocity;              // linearVelocity 말고 velocity 쓰는 게 일반적
        vel.x = targetVel.x;
        vel.z = targetVel.z;
        rb.linearVelocity = vel;

        if (move.sqrMagnitude > 0.01f)
        {
            Quaternion targetRot = Quaternion.LookRotation(move);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, 12f * Time.fixedDeltaTime);
        }

        if (move.magnitude > 0.1f)
            animancer.Play(walkClip, fade);
        else
            animancer.Play(idleClip, fade);
    }

    
    // private void FixedUpdate()
    // {
    //     Vector3 move = new Vector3(moveInput.x, 0, moveInput.y);
    //     // if (cameraPivot)
    //     // {
    //     //     Vector3 camForward = cameraPivot.forward;
    //     //     camForward.y = 0;
    //     //     Quaternion camRot = Quaternion.LookRotation(camForward);
    //     //     move = camRot * move;
    //     // }
    //
    //     Vector3 targetVel = move * moveSpeed;
    //     Vector3 vel = rb.linearVelocity;
    //     vel.x = targetVel.x;
    //     vel.z = targetVel.z;
    //     rb.linearVelocity = vel;
    //
    //     // 회전
    //     if (move.sqrMagnitude > 0.01f)
    //     {
    //         Quaternion targetRot = Quaternion.LookRotation(move);
    //         transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, 12f * Time.fixedDeltaTime);
    //     }
    //
    //     // 애니메이션
    //     if (move.magnitude > 0.1f)
    //         animancer.Play(walkClip, fade);
    //     else
    //         animancer.Play(idleClip, fade);
    // }

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


