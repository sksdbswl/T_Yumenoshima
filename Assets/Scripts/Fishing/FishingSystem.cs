using System.Collections;
using UnityEngine;

/// <summary>
/// 최소 구현 낚시 데모:
/// - WASD 이동(간단 CharacterController)
/// - 마우스 좌클릭/키로 단계 진행
///   Normal:   좌클릭/Enter → 캐스팅
///   Cast:     입질 시 Space로 훅킹(시간 제한)
///   Struggle: A/D로 방향 맞춰 게이지 채우기
///   Reel:     Space를 템포에 맞춰 여러 번 성공
///   Lift:     간단 연출 후 결과 출력
/// - 외부 의존(네트워크/시네머신/DOtween/에셋) 없음
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class SimpleFishingDemo : MonoBehaviour
{
    // ---- 이동 관련 ----
    [Header("Movement")]
    public float moveSpeed = 4.5f;
    public float gravity = -9.81f;
    public float jumpSpeed = 5f;            // 점프 필요 없으면 0
    public Transform cameraPivot;           // 없으면 월드 기준 WASD
    private CharacterController cc;
    private float verticalVel;

    // ---- 낚시 상태 ----
    private enum FishingState { Normal, Cast, Bite, Struggle, Reel, Lift }
    private FishingState state = FishingState.Normal;

    // ---- 캐스팅 & 입질 ----
    [Header("Casting/Bite")]
    public float biteMinDelay = 1.0f;       // 캐스팅 후 입질 최소 대기
    public float biteMaxDelay = 3.0f;       // 캐스팅 후 입질 최대 대기
    public float biteWindow = 1.8f;         // 입질 후 훅킹 가능 시간(Space)
    private float biteTimer;                // 입질까지 남은 시간
    private float biteWindowRemain;         // 훅킹 가능 남은 시간
    private bool isBiting;                  // 입질 중 표시

    // ---- 스트러글 ----
    [Header("Struggle")]
    public int struggleCheckCount = 3;      // 체크포인트 개수
    public int struggleTargetPerStage = 6;  // 스테이지마다 필요한 성공 수
    public float struggleDirChangeInterval = 1.0f; // 물고기 방향 바뀌는 주기
    private int struggleStage;              // 현재 몇 번째 체크포인트(0~)
    private int struggleProgress;           // 현 단계 누적 성공 수
    private float struggleDirTimer;         // 방향 전환 타이머
    private int fishDir;                    // -1(Left) / 0(None) / +1(Right)

    // ---- 릴(감기) ----
    [Header("Reel")]
    public int reelNeedSuccess = 5;         // 성공 횟수
    public float reelBeatInterval = 0.7f;   // 비트(리듬) 간격
    public float reelEarlyLate = 0.18f;     // 허용 오차
    private float reelBeatT;
    private int reelSuccess;
    private bool reelBeatUp;                // 내부 비트 표시(시각화용)

    // ---- 랜딩(리프트) ----
    [Header("Lift")]
    public float liftTime = 1.2f;
    private float liftT;

    // ---- 기타 ----
    private string lastResult = "";
    private bool showHelp = true;

    // -------------------- Unity --------------------
    private void Awake()
    {
        cc = GetComponent<CharacterController>();
    }

    private void Update()
    {
        HandleMovement();
        UpdateFishing(Time.deltaTime);
        if (Input.GetKeyDown(KeyCode.F1)) showHelp = !showHelp;
    }

    private void OnGUI()
    {
        DrawHUD();
    }

    // -------------------- 이동 --------------------
    private void HandleMovement()
    {
        if (state != FishingState.Normal)
        {
            // 낚시 중에도 움직이고 싶다면 이 가드 제거
            // return;
        }

        Vector3 input = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical"));
        input = input.normalized;

        Vector3 moveDir = input;
        if (cameraPivot != null)
        {
            // 카메라 기준 WASD
            Vector3 forward = cameraPivot.forward; forward.y = 0f; forward.Normalize();
            Vector3 right = cameraPivot.right; right.y = 0f; right.Normalize();
            moveDir = (forward * input.z + right * input.x);
        }

        if (cc.isGrounded)
        {
            verticalVel = 0f;
            if (jumpSpeed > 0f && Input.GetKeyDown(KeyCode.Space) && state == FishingState.Normal)
                verticalVel = jumpSpeed;
        }
        verticalVel += gravity * Time.deltaTime;

        Vector3 vel = moveDir * moveSpeed + Vector3.up * verticalVel;
        cc.Move(vel * Time.deltaTime);

        // 이동 방향으로 회전(선택)
        if (moveDir.sqrMagnitude > 0.0001f)
        {
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                Quaternion.LookRotation(moveDir, Vector3.up),
                12f * Time.deltaTime
            );
        }
    }

    // -------------------- 낚시 루프 --------------------
    private void UpdateFishing(float dt)
    {
        switch (state)
        {
            case FishingState.Normal:
                if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Return))
                {
                    StartCast();
                }
                break;

            case FishingState.Cast:
                biteTimer -= dt;
                if (biteTimer <= 0f && !isBiting)
                {
                    // 입질 시작
                    isBiting = true;
                    state = FishingState.Bite;
                    biteWindowRemain = biteWindow;
                }
                break;

            case FishingState.Bite:
                biteWindowRemain -= dt;
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    StartStruggle();
                }
                else if (biteWindowRemain <= 0f)
                {
                    MissAndReset("Missed the bite!");
                }
                break;

            case FishingState.Struggle:
                // 물고기 방향 주기적으로 랜덤 변경
                struggleDirTimer -= dt;
                if (struggleDirTimer <= 0f)
                {
                    struggleDirTimer = struggleDirChangeInterval;
                    fishDir = Random.value < 0.5f ? -1 : 1;
                }

                // 플레이어 입력(A/D)으로 같은 방향이면 성공 누적
                int inputDir =
                    Input.GetKeyDown(KeyCode.A) ? -1 :
                    Input.GetKeyDown(KeyCode.D) ? +1 : 0;

                if (inputDir != 0)
                {
                    if (inputDir == fishDir)
                    {
                        struggleProgress++;
                    }
                    else
                    {
                        // 틀리면 살짝 감점(선택 사항)
                        struggleProgress = Mathf.Max(0, struggleProgress - 1);
                    }
                }

                if (struggleProgress >= struggleTargetPerStage)
                {
                    struggleStage++;
                    if (struggleStage >= struggleCheckCount)
                    {
                        StartReel();
                    }
                    else
                    {
                        // 다음 단계 초기화
                        struggleProgress = 0;
                        fishDir = 0; // 잠깐 중립
                        struggleDirTimer = 0.3f;
                    }
                }
                break;

            case FishingState.Reel:
                // 비트 진행
                reelBeatT += dt;
                // UI 참고용 토글
                if (reelBeatT % reelBeatInterval < reelBeatInterval * 0.5f) reelBeatUp = true; else reelBeatUp = false;

                // Space를 beat 타이밍에 맞춰 누르면 성공
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    float mod = reelBeatT % reelBeatInterval;
                    float dist = Mathf.Min(mod, reelBeatInterval - mod); // 비트 최근접 거리
                    if (dist <= reelEarlyLate)
                    {
                        reelSuccess++;
                    }
                    else
                    {
                        // 실패 시 패널티(선택): 성공 수 감소
                        reelSuccess = Mathf.Max(0, reelSuccess - 1);
                    }
                }

                if (reelSuccess >= reelNeedSuccess)
                {
                    StartLift();
                }
                break;

            case FishingState.Lift:
                liftT += dt;
                if (liftT >= liftTime)
                {
                    CatchAndReset();
                }
                break;
        }
    }

    // -------------------- 단계 전환 --------------------
    private void StartCast()
    {
        state = FishingState.Cast;
        isBiting = false;
        biteTimer = Random.Range(biteMinDelay, biteMaxDelay);
        lastResult = "Casting...";
    }

    private void StartStruggle()
    {
        state = FishingState.Struggle;
        struggleStage = 0;
        struggleProgress = 0;
        fishDir = 0; // 시작은 잠깐 중립
        struggleDirTimer = 0.3f;
        lastResult = "Hooked! Struggle start";
    }

    private void StartReel()
    {
        state = FishingState.Reel;
        reelBeatT = 0f;
        reelSuccess = 0;
        lastResult = "Reel it in!";
    }

    private void StartLift()
    {
        state = FishingState.Lift;
        liftT = 0f;
        lastResult = "Lifting...";
    }

    private void CatchAndReset()
    {
        lastResult = "You caught a fish!";
        ResetToNormal();
    }

    private void MissAndReset(string msg)
    {
        lastResult = msg;
        ResetToNormal();
    }

    private void ResetToNormal()
    {
        state = FishingState.Normal;
        isBiting = false;
        biteTimer = 0f;
        biteWindowRemain = 0f;

        struggleStage = 0;
        struggleProgress = 0;
        fishDir = 0;

        reelBeatT = 0f;
        reelSuccess = 0;
        reelBeatUp = false;

        liftT = 0f;
    }

    // -------------------- HUD --------------------
    private void DrawHUD()
    {
        const int w = 340;
        Rect box = new Rect(16, 16, w, 220);
        GUI.Box(box, "Simple Fishing");

        float y = 40;
        GUI.Label(new Rect(26, y, w - 20, 22), $"State: {state}"); y += 22;
        GUI.Label(new Rect(26, y, w - 20, 22), $"Result: {lastResult}"); y += 22;

        switch (state)
        {
            case FishingState.Normal:
                GUI.Label(new Rect(26, y, w - 20, 22), "LMB / Enter: Cast"); y += 22;
                break;

            case FishingState.Cast:
                GUI.Label(new Rect(26, y, w - 20, 22), $"Waiting bite... ({biteTimer:0.00}s)"); y += 22;
                break;

            case FishingState.Bite:
                GUI.Label(new Rect(26, y, w - 20, 22), $"BITE! Space to hook ({biteWindowRemain:0.00}s)"); y += 22;
                break;

            case FishingState.Struggle:
                GUI.Label(new Rect(26, y, w - 20, 22), $"Struggle Stage: {struggleStage + 1}/{struggleCheckCount}"); y += 22;
                GUI.Label(new Rect(26, y, w - 20, 22), $"Progress: {struggleProgress}/{struggleTargetPerStage}"); y += 22;
                string dirTxt = fishDir == 0 ? " - " : (fishDir < 0 ? "<- LEFT" : "RIGHT ->");
                GUI.Label(new Rect(26, y, w - 20, 22), $"Fish Dir: {dirTxt} | Press A/D"); y += 22;
                break;

            case FishingState.Reel:
                GUI.Label(new Rect(26, y, w - 20, 22), $"Reel Success: {reelSuccess}/{reelNeedSuccess}"); y += 22;
                GUI.Label(new Rect(26, y, w - 20, 22), $"Press SPACE on beat (interval {reelBeatInterval:0.00}s)"); y += 22;

                // 간단한 비트 시각화 바
                float progress = (reelBeatT % reelBeatInterval) / reelBeatInterval;
                GUI.HorizontalScrollbar(new Rect(26, y, w - 40, 18), progress, 0.05f, 0f, 1f); y += 26;
                GUI.Label(new Rect(26, y, w - 20, 22), reelBeatUp ? "▲ beat" : "▽ beat"); y += 22;
                break;

            case FishingState.Lift:
                GUI.Label(new Rect(26, y, w - 20, 22), $"Lifting... ({liftT:0.00}/{liftTime:0.00})"); y += 22;
                break;
        }

        if (showHelp)
        {
            y += 6;
            GUI.Box(new Rect(16, y, w, 120), "Controls");
            float y2 = y + 24;
            GUI.Label(new Rect(26, y2, w - 20, 20), "WASD : Move (카메라 기준 회전 지원)"); y2 += 18;
            GUI.Label(new Rect(26, y2, w - 20, 20), "LMB/Enter : Cast (Normal)"); y2 += 18;
            GUI.Label(new Rect(26, y2, w - 20, 20), "Space : Hook(Reel 비트 입력도 Space)"); y2 += 18;
            GUI.Label(new Rect(26, y2, w - 20, 20), "A/D : Struggle 방향 입력"); y2 += 18;
            GUI.Label(new Rect(26, y2, w - 20, 20), "F1 : 도움말 토글"); y2 += 18;
        }
    }
}
