using UnityEngine;

public class scooterCtrl : MonoBehaviour
{
    [Header("스쿠터 설정")]
    public float maxSpeed = 30f;           // 최대 속도
    public float acceleration = 0.1f;        // 가속도 (8f → 4f로 감소)
    public float deceleration = 10f;       // 감속도
    public float turnSpeed = 80f;          // 회전 속도 (줄임)
    public float turnSmoothness = 5f;      // 회전 부드러움
    public float brakeForce = 20f;         // 브레이크 힘

    [Header("드리프트 시스템")]
    public float driftSensitivity = 1.3f;  // 드리프트 민감도 (줄임)
    public float driftForce = 15f;         // 드리프트 힘
    public float driftGaugeRate = 1f;      // 게이지 충전 속도
    public float maxDriftGauge = 100f;     // 최대 드리프트 게이지

    [Header("부스터 시스템")]
    public float boostSpeed = 25f;         // 최대 부스터 속도
    public float boostDuration = 2f;       // 부스터 지속 시간
    public float minBoostGauge = 25f;      // 최소 부스터 사용 게이지
    public float maxBoostGauge = 100f;     // 최대 부스터 게이지
    public ParticleSystem[] boostEffects;  // 부스터 파티클 효과

    [Header("물리 설정")]
    public float gravity = 20f;            // 중력
    public float groundCheckDistance = 1.1f; // 지면 체크 거리

    [Header("오디오 (선택사항)")]
    public AudioSource engineSound;        // 엔진 사운드

    private CharacterController controller;
    private Vector3 velocity;
    private float currentSpeed;
    private bool isGrounded;
    private float motorInput;
    private float steerInput;
    private bool isBraking;

    // 부드러운 회전을 위한 변수
    private float currentTurnInput;
    private float targetTurnInput;

    // 드리프트 및 부스터 변수들
    private bool isDrifting;
    private bool driftInput;
    private bool boostInput;
    private float driftGauge;
    private bool isBoosting;
    private float boostTimer;
    private Vector3 driftDirection;
    private float driftAngle;
    private float currentBoostPower;  // 현재 부스터 파워 (0-1)
    private float usedBoostGauge;     // 사용된 부스터 게이지

    void Start()
    {
        controller = GetComponent<CharacterController>();

        // CharacterController가 없으면 추가
        if (controller == null)
        {
            controller = gameObject.AddComponent<CharacterController>();
            controller.radius = 0.5f;
            controller.height = 1.8f;
            controller.center = new Vector3(0, 0.9f, 0);
        }

        velocity = Vector3.zero;
        currentSpeed = 0f;
        driftGauge = 0f;
        isDrifting = false;
        isBoosting = false;
    }

    void Update()
    {
        HandleInput();
        HandleDrift();
        HandleBoost();
        HandleMovement();
        HandleAudio();
        UpdateEffects();
    }

    void HandleInput()
    {
        // 기본 입력 받기
        motorInput = Input.GetAxis("Vertical");     // W/S 또는 위/아래 화살표
        steerInput = Input.GetAxis("Horizontal");   // A/D 또는 좌/우 화살표
        isBraking = Input.GetKey(KeyCode.Space);    // 스페이스바로 브레이크

        // 드리프트 입력 (Shift)
        driftInput = Input.GetKey(KeyCode.LeftShift);

        // 부스터 입력 (Ctrl)
        boostInput = Input.GetKeyDown(KeyCode.LeftControl);

        // 추가 컨트롤 (선택사항)
        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetScooter();
        }
    }

    void HandleMovement()
    {
        // 지면 체크
        isGrounded = controller.isGrounded;

        // 모터 입력 처리
        if (motorInput > 0)
        {
            // 전진 가속
            currentSpeed += acceleration * motorInput * Time.deltaTime;
        }
        else if (motorInput < 0)
        {
            // 후진 (속도 제한)
            currentSpeed += acceleration * motorInput * 0.5f * Time.deltaTime;
        }
        else if (!isBraking)
        {
            // 자연 감속
            currentSpeed = Mathf.Lerp(currentSpeed, 0, deceleration * 0.1f * Time.deltaTime);
        }

        // 브레이크 처리
        if (isBraking)
        {
            currentSpeed = Mathf.Lerp(currentSpeed, 0, brakeForce * Time.deltaTime);
        }

        // 속도 제한
        currentSpeed = Mathf.Clamp(currentSpeed, -maxSpeed * 0.5f, maxSpeed);

        // 부스터 속도 적용 (게이지에 따른 가변 속도)
        if (isBoosting && currentSpeed > 0)
        {
            float boostSpeedBonus = (boostSpeed - maxSpeed) * currentBoostPower;
            float effectiveMaxSpeed = maxSpeed + boostSpeedBonus;
            currentSpeed = Mathf.Min(currentSpeed, effectiveMaxSpeed);
        }

        // 부드러운 회전 처리
        targetTurnInput = steerInput;
        currentTurnInput = Mathf.Lerp(currentTurnInput, targetTurnInput, turnSmoothness * Time.deltaTime);

        // 회전 처리 (속도에 따라 회전력 조절)
        if (Mathf.Abs(currentSpeed) > 0.1f && Mathf.Abs(currentTurnInput) > 0.05f)
        {
            // 속도가 빠를수록 회전이 더 민감하게, 하지만 제한적으로
            float speedFactor = Mathf.Clamp01(Mathf.Abs(currentSpeed) / maxSpeed);
            float turnMultiplier = Mathf.Lerp(0.3f, 1f, speedFactor); // 최소 30%, 최대 100%

            float turn = currentTurnInput * turnSpeed * turnMultiplier * Time.deltaTime;

            // 드리프트 중일 때는 회전이 더 급격함 (하지만 매우 부드럽게)
            if (isDrifting)
            {
                turn *= driftSensitivity * 0.8f; // 드리프트 감도를 더 줄임
                driftAngle = currentTurnInput * 20f; // 드리프트 각도를 더 줄임 (30도→20도)
            }

            transform.Rotate(0, turn, 0);
        }

        // 이동 벡터 계산
        Vector3 move;

        if (isDrifting && Mathf.Abs(currentTurnInput) > 0.1f)
        {
            // 드리프트 중: 앞으로 가면서 옆으로 밀림 (더욱 부드럽게)
            Vector3 forward = transform.forward * currentSpeed;
            Vector3 side = transform.right * (currentTurnInput * driftForce * 0.5f * Time.deltaTime); // 드리프트 힘을 더 줄임
            move = forward + side;
        }
        else
        {
            // 일반 이동
            move = transform.forward * currentSpeed;
        }

        // 중력 적용
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f; // 지면에 붙어있기
        }
        else
        {
            velocity.y -= gravity * Time.deltaTime;
        }

        // 최종 이동
        Vector3 finalMove = move + Vector3.up * velocity.y;
        controller.Move(finalMove * Time.deltaTime);
    }

    void HandleAudio()
    {
        // 엔진 사운드 처리 (AudioSource가 있는 경우)
        if (engineSound != null)
        {
            float speedRatio = Mathf.Abs(currentSpeed) / maxSpeed;
            engineSound.pitch = Mathf.Lerp(0.8f, 2.0f, speedRatio);
            engineSound.volume = Mathf.Lerp(0.3f, 1.0f, speedRatio);

            if (speedRatio > 0.1f && !engineSound.isPlaying)
            {
                engineSound.Play();
            }
            else if (speedRatio <= 0.1f && engineSound.isPlaying)
            {
                engineSound.Stop();
            }
        }
    }

    void HandleDrift()
    {
        // 드리프트 조건 체크 (더 부드러운 조건)
        bool canDrift = driftInput && Mathf.Abs(steerInput) > 0.2f && currentSpeed > 2f;

        if (canDrift && !isDrifting)
        {
            // 드리프트 시작
            isDrifting = true;
            driftDirection = transform.right * Mathf.Sign(steerInput);

            Debug.Log("🏎️ 드리프트 시작!");
        }
        else if (!driftInput || Mathf.Abs(steerInput) < 0.15f || currentSpeed < 1.5f)
        {
            // 드리프트 종료 (더 부드러운 종료 조건)
            if (isDrifting)
            {
                Debug.Log("🏁 드리프트 종료!");
            }
            isDrifting = false;
        }

        // 드리프트 게이지 업데이트
        if (isDrifting)
        {
            // 드리프트 중: 게이지 충전
            float chargeRate = driftGaugeRate * Mathf.Abs(steerInput) * (currentSpeed / maxSpeed);
            driftGauge += chargeRate * Time.deltaTime * 15f; // 20배에서 15배로 줄임
            driftGauge = Mathf.Clamp(driftGauge, 0f, maxDriftGauge);

            // 게이지가 찰 때마다 알림
            if (driftGauge >= maxDriftGauge && !isBoosting)
            {
                Debug.Log("⚡ 부스터 준비 완료! [Ctrl]을 눌러 부스터 사용!");
            }
        }
        else
        {
            // 드리프트 중이 아닐 때: 게이지 천천히 감소 (매우 느리게)
            driftGauge -= 2f * Time.deltaTime; // 8에서 2로 변경 (4배 느려짐)
            driftGauge = Mathf.Max(0f, driftGauge);
        }
    }

    void HandleBoost()
    {
        // 부스터 활성화 (최소 게이지 25% 이상 필요)
        if (boostInput && driftGauge >= minBoostGauge && !isBoosting)
        {
            StartBoost();
        }

        // 부스터 타이머 업데이트
        if (isBoosting)
        {
            boostTimer -= Time.deltaTime;
            if (boostTimer <= 0f)
            {
                EndBoost();
            }
        }
    }

    void StartBoost()
    {
        isBoosting = true;

        // 게이지에 따른 부스터 파워 계산 (25% ~ 100%)
        currentBoostPower = Mathf.Clamp01(driftGauge / maxBoostGauge);
        usedBoostGauge = driftGauge; // 사용할 게이지 저장

        // 부스터 지속 시간도 게이지에 따라 조절
        boostTimer = boostDuration * currentBoostPower;

        // 게이지 소모
        driftGauge = 0f;

        string powerPercent = (currentBoostPower * 100f).ToString("F0");
        Debug.Log($"🚀 부스터 발동! 파워: {powerPercent}% ({boostTimer:F1}초간)");

        // 부스터 이펙트 활성화
        if (boostEffects != null)
        {
            foreach (var effect in boostEffects)
            {
                if (effect != null)
                {
                    effect.Play();
                }
            }
        }
    }

    void EndBoost()
    {
        isBoosting = false;
        Debug.Log("⏰ 부스터 종료!");

        // 부스터 이펙트 비활성화
        if (boostEffects != null)
        {
            foreach (var effect in boostEffects)
            {
                if (effect != null)
                {
                    effect.Stop();
                }
            }
        }
    }

    void UpdateEffects()
    {
        // 엔진 사운드 업데이트 (부스터 중일 때 더 높은 피치)
        if (engineSound != null)
        {
            float currentMaxSpeed = maxSpeed;
            if (isBoosting)
            {
                float boostSpeedBonus = (boostSpeed - maxSpeed) * currentBoostPower;
                currentMaxSpeed = maxSpeed + boostSpeedBonus;
            }

            float speedRatio = Mathf.Abs(currentSpeed) / currentMaxSpeed;
            float pitchMultiplier = isBoosting ? (1f + currentBoostPower * 0.5f) : 1f;
            engineSound.pitch = Mathf.Lerp(0.8f, 2.5f, speedRatio) * pitchMultiplier;
        }
    }

    void ResetScooter()
    {
        // 스쿠터 리셋 (넘어졌을 때 사용)
        currentSpeed = 0f;
        velocity = Vector3.zero;
        transform.rotation = Quaternion.Euler(0, transform.eulerAngles.y, 0);
    }

    // 현재 속도를 외부에서 확인할 수 있는 프로퍼티
    public float CurrentSpeed => currentSpeed;
    public bool IsGrounded => isGrounded;
    public bool IsBraking => isBraking;

    // 디버그 UI 표시
    void OnGUI()
    {
        // 스타일 설정
        GUIStyle boxStyle = new GUIStyle(GUI.skin.box);
        boxStyle.fontSize = 14;
        boxStyle.normal.textColor = Color.white;

        GUIStyle labelStyle = new GUIStyle(GUI.skin.label);
        labelStyle.fontSize = 12;
        labelStyle.normal.textColor = Color.white;

        // 정보 박스
        GUI.Box(new Rect(10, 10, 250, 150), "🏍️ 카트라이더 스쿠터 정보", boxStyle);

        // 속도 정보
        float displayMaxSpeed = maxSpeed;
        if (isBoosting)
        {
            float boostSpeedBonus = (boostSpeed - maxSpeed) * currentBoostPower;
            displayMaxSpeed = maxSpeed + boostSpeedBonus;
        }
        GUI.Label(new Rect(20, 35, 200, 20),
            $"속도: {currentSpeed:F1} / {displayMaxSpeed:F1} km/h", labelStyle);

        // 드리프트 게이지
        GUI.Label(new Rect(20, 55, 200, 20),
            $"드리프트 게이지: {driftGauge:F1} / {maxDriftGauge:F1}", labelStyle);

        // 드리프트 게이지 바
        Rect gaugeRect = new Rect(20, 75, 200, 10);
        GUI.Box(gaugeRect, "");

        // 게이지 채우기 (25% 이상에서 색상 변경)
        float gaugeFill = driftGauge / maxDriftGauge;
        Color gaugeColor;
        if (gaugeFill >= 1f)
            gaugeColor = Color.yellow;      // 100%: 노란색
        else if (gaugeFill >= 0.25f)
            gaugeColor = Color.green;       // 25% 이상: 초록색
        else
            gaugeColor = Color.cyan;        // 25% 미만: 청록색

        GUI.color = gaugeColor;
        GUI.Box(new Rect(gaugeRect.x, gaugeRect.y, gaugeRect.width * gaugeFill, gaugeRect.height), "");

        // 25% 구분선 표시
        GUI.color = Color.white;
        float minGaugeLine = (minBoostGauge / maxDriftGauge) * gaugeRect.width;
        GUI.Box(new Rect(gaugeRect.x + minGaugeLine - 1, gaugeRect.y, 2, gaugeRect.height), "");
        GUI.color = Color.white;

        // 상태 정보
        GUI.Label(new Rect(20, 95, 200, 20),
            $"드리프트: {(isDrifting ? "ON" : "OFF")}", labelStyle);

        GUI.Label(new Rect(20, 115, 200, 20),
            $"부스터: {(isBoosting ? $"ON {currentBoostPower * 100:F0}% ({boostTimer:F1}s)" : "OFF")}", labelStyle);

        GUI.Label(new Rect(20, 135, 200, 20),
            $"지면: {(isGrounded ? "착지" : "공중")}", labelStyle);

        // 조작법 안내
        GUI.Box(new Rect(10, 170, 300, 100), "🎮 조작법", boxStyle);
        GUI.Label(new Rect(20, 195, 280, 20), "WASD: 이동 / Shift: 드리프트", labelStyle);
        GUI.Label(new Rect(20, 215, 280, 20), "Ctrl: 부스터 (25% 이상 필요)", labelStyle);
        GUI.Label(new Rect(20, 235, 280, 20), "R: 리셋", labelStyle);
        GUI.Label(new Rect(20, 255, 280, 20), "💡 게이지가 많을수록 강한 부스터!", labelStyle);
    }

    // 기즈모로 지면 체크 영역 표시 (에디터에서만)
    void OnDrawGizmosSelected()
    {
        Gizmos.color = isGrounded ? Color.green : Color.red;
        Gizmos.DrawRay(transform.position, Vector3.down * groundCheckDistance);
    }
}
