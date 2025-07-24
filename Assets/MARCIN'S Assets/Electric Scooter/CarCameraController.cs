using UnityEngine;
using System.Collections;

public class CarCameraController : MonoBehaviour
{
    [Header("카메라 타겟")]
    public Transform carTransform;              // 따라갈 차량
    public SportsCarController carController;   // 차량 컨트롤러 참조

    [Header("카메라 모드")]
    public CameraMode currentMode = CameraMode.ThirdPerson;

    [Header("3인칭 카메라 설정")]
    public Vector3 thirdPersonOffset = new Vector3(0, 2.5f, -4f);  // 거리 단축 (-6 → -4)
    public float thirdPersonHeight = 1.5f;
    public float thirdPersonDistance = 4f;                         // 거리 단축 (6 → 4)

    [Header("1인칭 카메라 설정")]
    public Vector3 firstPersonOffset = new Vector3(0, 0.6f, 0.5f);

    [Header("탑뷰 카메라 설정")]
    public Vector3 topViewOffset = new Vector3(0, 15f, -5f);
    public float topViewAngle = 60f;

    [Header("시네마틱 카메라 설정")]
    public Vector3 cinematicOffset = new Vector3(3f, 1f, -8f);
    public float cinematicRotationSpeed = 0.5f;

    [Header("카메라 움직임 설정")]
    public float followSmoothness = 5f;         // 따라가기 부드러움 (더 부드럽게)
    public float rotationSmoothness = 8f;       // 회전 부드러움 (더 부드럽게)
    public float speedBasedDistance = 0.02f;   // 속도에 따른 거리 조정 (대폭 감소)
    public float maxSpeedDistance = 1f;         // 최대 추가 거리 (감소)

    [Header("카메라 쉐이크")]
    public bool enableCameraShake = true;
    public float shakeIntensity = 0.02f;        // 쉐이크 강도 대폭 줄임 (0.1 → 0.02)
    public float maxShakeSpeed = 150f;          // 최대 쉐이크 속도 증가
    public float shakeSmoothing = 5f;           // 쉐이크 부드러움 추가

    [Header("자동 카메라 전환")]
    public bool autoSwitchCamera = false;
    public float highSpeedThreshold = 80f;      // 자동 전환 속도

    [Header("룩어헤드 설정")]
    public bool enableLookAhead = true;
    public float lookAheadDistance = 5f;
    public float lookAheadSmoothness = 2f;

    [Header("🆕 커스텀 카메라 무빙")]
    public bool useCustomCameraMoving = false;      // 커스텀 카메라 무빙 사용 여부
    public Transform[] cameraWaypoints;             // 카메라가 이동할 경로 포인트들
    public float customMovingSpeed = 2f;            // 🆕 커스텀 무빙 속도 (단위/초)
    public bool useConstantSpeed = true;            // 🆕 일정한 속도 사용 여부
    public float customMovingDuration = 8f;         // 시간 기반일 때 사용 (useConstantSpeed가 false일 때)
    public AnimationCurve movingCurve = AnimationCurve.EaseInOut(0, 0, 1, 1); // 이동 곡선
    public bool lookAtPlayerDuringMoving = false;   // 🆕 무빙 중 플레이어를 바라볼지 여부 (기본값 false로 변경)

    [Header("🎬 카메라 무빙 부드러움 설정")]
    public float positionSmoothness = 10f;          // 🆕 위치 이동 부드러움 (높을수록 부드러움)
    public float customRotationSmoothness = 8f;     // 🆕 커스텀 무빙 회전 부드러움 (높을수록 부드러움)
    public AnimationCurve easeInOutCurve = new AnimationCurve(
        new Keyframe(0f, 0f, 0f, 2f),      // 시작: 천천히
        new Keyframe(0.5f, 0.5f, 2f, 2f),  // 중간: 빠르게
        new Keyframe(1f, 1f, 2f, 0f)       // 끝: 천천히
    ); // 🆕 더욱 부드러운 이징 곡선

    // 프라이빗 변수들
    private Camera cameraComponent;
    private Vector3 currentVelocity;
    private Vector3 targetPosition;
    private Vector3 lookAheadTarget;
    private float originalFOV;
    private Vector3 shakeOffset;
    private float lastSwitchTime;
    private bool isGameStarted = false;     // 🆕 게임 시작 여부
    private Vector3 initialCameraPosition;  // 🆕 초기 카메라 위치
    private bool isCustomMoving = false;    // 🆕 커스텀 무빙 중인지 여부
    private Coroutine customMovingCoroutine; // 🆕 커스텀 무빙 코루틴

    // 🆕 카메라 무빙 완료 이벤트
    public System.Action OnCustomCameraMovingComplete;

    // 카메라 모드 열거형
    public enum CameraMode
    {
        FirstPerson,    // 1인칭 (차량 내부)
        ThirdPerson,    // 3인칭 (차량 뒤)
        TopView,        // 탑뷰 (위에서 내려다보기)
        Cinematic,      // 시네마틱 (회전하는 카메라)
        Free            // 자유 카메라
    }

    void Start()
    {
        cameraComponent = GetComponent<Camera>();
        originalFOV = cameraComponent.fieldOfView;

        // 🆕 초기 카메라 위치 저장
        initialCameraPosition = transform.position;

        // 차량을 자동으로 찾기
        if (carTransform == null)
        {
            GameObject car = GameObject.FindWithTag("Player");
            if (car != null)
            {
                carTransform = car.transform;
                carController = car.GetComponent<SportsCarController>();
            }
        }

        if (carTransform == null)
        {
            Debug.LogWarning("Car Transform이 설정되지 않았습니다!");
        }

        // 🆕 게임 시작 전에는 카메라 무빙 비활성화
        isGameStarted = false;
        Debug.Log("📹 카메라 대기 중... 게임 시작을 기다립니다.");
    }

    void LateUpdate()
    {
        // 커스텀 무빙 중 P키로 스킵 (가장 먼저 체크)
        if (isCustomMoving && Input.GetKeyDown(KeyCode.P))
        {
            SkipCustomCameraMoving();
            return;
        }

        // 🆕 게임이 시작되지 않았거나 차량이 없으면 카메라 무빙 중단
        if (carTransform == null || !isGameStarted) return;

        // 🆕 커스텀 무빙 중이면 일반 카메라 업데이트 완전 차단
        if (isCustomMoving)
        {
            // 커스텀 무빙 중에는 아무것도 실행하지 않음 (완전 차단)
            return;
        }

        HandleInput();
        UpdateCameraPosition();
        UpdateCameraShake();
        UpdateFieldOfView();
        AutoSwitchCamera();
    }

    /// <summary>
    /// 🚀 게임 시작 시 카메라 무빙 활성화
    /// </summary>
    public void StartGameCamera()
    {
        isGameStarted = true;

        // 🔍 디버깅 정보 출력
        Debug.Log($"🔍 카메라 설정 확인:");
        Debug.Log($"  - useCustomCameraMoving: {useCustomCameraMoving}");
        Debug.Log($"  - useConstantSpeed: {useConstantSpeed}");
        Debug.Log($"  - customMovingSpeed: {customMovingSpeed} units/sec");
        Debug.Log($"  - lookAtPlayerDuringMoving: {lookAtPlayerDuringMoving}");
        Debug.Log($"  - cameraWaypoints 길이: {(cameraWaypoints != null ? cameraWaypoints.Length : 0)}");

        // 🆕 커스텀 카메라 무빙 사용 여부 확인
        if (useCustomCameraMoving && cameraWaypoints != null && cameraWaypoints.Length > 0)
        {
            // 커스텀 카메라 무빙 시작
            customMovingCoroutine = StartCoroutine(CustomCameraMoving());
            Debug.Log("📹 커스텀 카메라 무빙 시작!");
        }
        else
        {
            // 기본 3인칭 카메라로 설정
            SetCameraMode(CameraMode.ThirdPerson);
            Debug.Log("📹 기본 카메라 무빙 시작!");
        }
    }

    /// <summary>
    /// 🎬 커스텀 카메라 무빙 코루틴 (속도 기반 개선)
    /// </summary>
    IEnumerator CustomCameraMoving()
    {
        isCustomMoving = true;
        float elapsedTime = 0f;
        float totalDuration;

        Debug.Log($"🎬 커스텀 카메라 무빙 시작! 경로 포인트: {cameraWaypoints.Length}개");
        Debug.Log($"🔍 lookAtPlayerDuringMoving 설정: {lookAtPlayerDuringMoving}");

        // 🆕 속도 기반 vs 시간 기반 선택
        if (useConstantSpeed)
        {
            float totalPathLength = CalculateTotalPathLength();
            totalDuration = totalPathLength / customMovingSpeed;
            Debug.Log($"🏃‍♂️ 일정 속도 모드: 경로 길이 {totalPathLength:F2}, 예상 시간 {totalDuration:F2}초");
        }
        else
        {
            totalDuration = customMovingDuration;
            Debug.Log($"⏰ 시간 기반 모드: 총 {totalDuration}초");
        }

        // 🆕 시작 위치와 회전 저장
        Vector3 startPosition = transform.position;
        Quaternion startRotation = transform.rotation;

        while (elapsedTime < totalDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / totalDuration;

            // 🆕 일정 속도일 때는 선형 progress, 시간 기반일 때는 곡선 적용
            float smoothProgress;
            if (useConstantSpeed)
            {
                smoothProgress = progress; // 선형 진행
            }
            else
            {
                smoothProgress = easeInOutCurve.Evaluate(progress); // 곡선 적용
            }

            // 🆕 목표 위치와 회전 계산
            Vector3 targetPosition = GetPositionAlongPath(smoothProgress);
            Quaternion targetRotation = GetRotationAlongPath(smoothProgress);

            // 🆕 부드러운 위치 이동
            if (useConstantSpeed)
            {
                // 일정 속도일 때는 직접 설정 (더 정확함)
                transform.position = targetPosition;
            }
            else
            {
                // 시간 기반일 때는 부드러운 보간
                transform.position = Vector3.Slerp(transform.position, targetPosition,
                    positionSmoothness * Time.deltaTime);
            }

            // 🆕 부드러운 회전
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation,
                customRotationSmoothness * Time.deltaTime);

            // 🛑 플레이어 추적 완전 차단 로그 (5초마다 한 번씩)
            if (Time.frameCount % 300 == 0) // 5초마다 한 번씩 로그
            {
                Debug.Log($"🎬 카메라 무빙 중... 진행률: {progress:F2}, 위치: {transform.position}");
            }

            yield return null;
        }

        // 🆕 마지막 위치와 회전을 정확히 설정
        if (cameraWaypoints.Length > 0)
        {
            transform.position = cameraWaypoints[cameraWaypoints.Length - 1].position;
            transform.rotation = cameraWaypoints[cameraWaypoints.Length - 1].rotation;
        }

        // 커스텀 무빙 완료 후 일반 카메라 모드로 전환
        isCustomMoving = false;
        SetCameraMode(CameraMode.ThirdPerson);

        Debug.Log("✨ 커스텀 카메라 무빙 완료! 일반 카메라 모드로 전환");

        // 🆕 카메라 무빙 완료 이벤트 호출
        OnCustomCameraMovingComplete?.Invoke();
    }

    /// <summary>
    /// 🆕 전체 경로 길이 계산
    /// </summary>
    float CalculateTotalPathLength()
    {
        if (cameraWaypoints == null || cameraWaypoints.Length < 2) return 0f;

        float totalLength = 0f;

        for (int i = 0; i < cameraWaypoints.Length - 1; i++)
        {
            if (cameraWaypoints[i] != null && cameraWaypoints[i + 1] != null)
            {
                totalLength += Vector3.Distance(cameraWaypoints[i].position, cameraWaypoints[i + 1].position);
            }
        }

        return totalLength;
    }

    /// <summary>
    /// 🛤️ 경로를 따라 위치 계산 (부드러움 개선)
    /// </summary>
    Vector3 GetPositionAlongPath(float progress)
    {
        if (cameraWaypoints.Length == 0) return transform.position;
        if (cameraWaypoints.Length == 1) return cameraWaypoints[0].position;

        // 전체 경로를 progress(0~1)에 따라 계산
        float scaledProgress = progress * (cameraWaypoints.Length - 1);
        int index = Mathf.FloorToInt(scaledProgress);
        float localProgress = scaledProgress - index;

        // 마지막 인덱스 처리
        if (index >= cameraWaypoints.Length - 1)
        {
            return cameraWaypoints[cameraWaypoints.Length - 1].position;
        }

        // 🆕 3개 이상 포인트가 있을 때 스플라인 보간 사용
        if (cameraWaypoints.Length >= 3)
        {
            return CalculateCatmullRomSpline(index, localProgress);
        }

        // 2개 포인트일 때는 기본 선형 보간
        Vector3 startPos = cameraWaypoints[index].position;
        Vector3 endPos = cameraWaypoints[index + 1].position;

        // 🆕 SmoothStep을 사용하여 더 부드러운 보간
        float smoothT = Mathf.SmoothStep(0f, 1f, localProgress);
        return Vector3.Lerp(startPos, endPos, smoothT);
    }

    /// <summary>
    /// 🆕 Catmull-Rom 스플라인을 사용한 부드러운 경로 계산
    /// </summary>
    Vector3 CalculateCatmullRomSpline(int index, float t)
    {
        // 4개의 점이 필요한 Catmull-Rom 스플라인을 위한 점들 준비
        Vector3 p0 = cameraWaypoints[Mathf.Max(0, index - 1)].position;
        Vector3 p1 = cameraWaypoints[index].position;
        Vector3 p2 = cameraWaypoints[Mathf.Min(cameraWaypoints.Length - 1, index + 1)].position;
        Vector3 p3 = cameraWaypoints[Mathf.Min(cameraWaypoints.Length - 1, index + 2)].position;

        // Catmull-Rom 스플라인 계산
        float t2 = t * t;
        float t3 = t2 * t;

        Vector3 result = 0.5f * (
            (2f * p1) +
            (-p0 + p2) * t +
            (2f * p0 - 5f * p1 + 4f * p2 - p3) * t2 +
            (-p0 + 3f * p1 - 3f * p2 + p3) * t3
        );

        return result;
    }

    /// <summary>
    /// 🆕 경로를 따라 회전 계산 (부드러움 개선)
    /// </summary>
    Quaternion GetRotationAlongPath(float progress)
    {
        if (cameraWaypoints.Length == 0) return transform.rotation;
        if (cameraWaypoints.Length == 1) return cameraWaypoints[0].rotation;

        // 전체 경로를 progress(0~1)에 따라 계산
        float scaledProgress = progress * (cameraWaypoints.Length - 1);
        int index = Mathf.FloorToInt(scaledProgress);
        float localProgress = scaledProgress - index;

        // 마지막 인덱스 처리
        if (index >= cameraWaypoints.Length - 1)
        {
            return cameraWaypoints[cameraWaypoints.Length - 1].rotation;
        }

        // 두 포인트 사이를 구면 보간 (더 부드럽게)
        Quaternion startRot = cameraWaypoints[index].rotation;
        Quaternion endRot = cameraWaypoints[index + 1].rotation;

        // 🆕 SmoothStep을 사용하여 더 부드러운 회전 보간
        float smoothT = Mathf.SmoothStep(0f, 1f, localProgress);
        return Quaternion.Slerp(startRot, endRot, smoothT);
    }

    /// <summary>
    /// 🛑 게임 종료 시 카메라 무빙 비활성화
    /// </summary>
    public void StopGameCamera()
    {
        isGameStarted = false;

        // 🆕 커스텀 무빙 중단
        if (customMovingCoroutine != null)
        {
            StopCoroutine(customMovingCoroutine);
            customMovingCoroutine = null;
        }

        isCustomMoving = false;

        Debug.Log("📹 게임 종료! 카메라 무빙 비활성화!");
    }

    void HandleInput()
    {
        // 카메라 모드 전환 (C키)
        if (Input.GetKeyDown(KeyCode.C))
        {
            SwitchToNextCamera();
        }

        // 특정 카메라 모드로 전환
        if (Input.GetKeyDown(KeyCode.Alpha1))
            SetCameraMode(CameraMode.FirstPerson);
        if (Input.GetKeyDown(KeyCode.Alpha2))
            SetCameraMode(CameraMode.ThirdPerson);
        if (Input.GetKeyDown(KeyCode.Alpha3))
            SetCameraMode(CameraMode.TopView);
        if (Input.GetKeyDown(KeyCode.Alpha4))
            SetCameraMode(CameraMode.Cinematic);
        if (Input.GetKeyDown(KeyCode.Alpha5))
            SetCameraMode(CameraMode.Free);
    }

    void UpdateCameraPosition()
    {
        switch (currentMode)
        {
            case CameraMode.FirstPerson:
                UpdateFirstPersonCamera();
                break;
            case CameraMode.ThirdPerson:
                UpdateThirdPersonCamera();
                break;
            case CameraMode.TopView:
                UpdateTopViewCamera();
                break;
            case CameraMode.Cinematic:
                UpdateCinematicCamera();
                break;
            case CameraMode.Free:
                UpdateFreeCamera();
                break;
        }
    }

    void UpdateFirstPersonCamera()
    {
        // 1인칭: 차량 내부 시점
        Vector3 targetPos = carTransform.TransformPoint(firstPersonOffset);
        transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref currentVelocity, 1f / followSmoothness);

        // 차량과 같은 방향으로 회전
        Quaternion targetRotation = carTransform.rotation;
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSmoothness * Time.deltaTime);
    }

    void UpdateThirdPersonCamera()
    {
        // 속도에 따른 거리 조정 (더 제한적으로)
        float speedFactor = 0f;
        if (carController != null)
        {
            // 속도 기준을 더 높여서 고속에서만 거리 증가
            speedFactor = Mathf.Clamp01(carController.CurrentSpeed / 150f);
        }

        float dynamicDistance = thirdPersonDistance + (speedFactor * speedBasedDistance * maxSpeedDistance);
        Vector3 offset = new Vector3(thirdPersonOffset.x, thirdPersonOffset.y, -dynamicDistance);

        // 룩어헤드 기능
        Vector3 lookTarget = carTransform.position;
        if (enableLookAhead && carController != null)
        {
            Vector3 velocity = carController.transform.GetComponent<Rigidbody>().linearVelocity;
            lookAheadTarget = Vector3.Slerp(lookAheadTarget, carTransform.position + velocity.normalized * lookAheadDistance,
                                          lookAheadSmoothness * Time.deltaTime);
            lookTarget = lookAheadTarget;
        }

        // 카메라 위치 계산
        Vector3 targetPos = carTransform.TransformPoint(offset);
        transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref currentVelocity, 1f / followSmoothness);

        // 카메라가 차량을 바라보도록 회전
        Vector3 lookDirection = lookTarget - transform.position;
        if (lookDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSmoothness * Time.deltaTime);
        }
    }

    void UpdateTopViewCamera()
    {
        // 탑뷰: 위에서 내려다보는 시점
        Vector3 targetPos = carTransform.position + topViewOffset;
        transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref currentVelocity, 1f / followSmoothness);

        // 아래를 내려다보는 각도
        Vector3 lookDirection = carTransform.position - transform.position;
        Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSmoothness * Time.deltaTime);
    }

    void UpdateCinematicCamera()
    {
        // 시네마틱: 차량 주위를 회전하는 카메라
        float angle = Time.time * cinematicRotationSpeed;
        Vector3 offset = new Vector3(
            Mathf.Sin(angle) * cinematicOffset.x,
            cinematicOffset.y,
            Mathf.Cos(angle) * cinematicOffset.z
        );

        Vector3 targetPos = carTransform.position + offset;
        transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref currentVelocity, 1f / followSmoothness);

        // 항상 차량을 바라보도록
        Vector3 lookDirection = carTransform.position - transform.position;
        Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSmoothness * Time.deltaTime);
    }

    void UpdateFreeCamera()
    {
        // 자유 카메라: 마우스로 조작 가능
        if (Input.GetMouseButton(1)) // 우클릭 드래그
        {
            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");

            transform.Rotate(-mouseY, mouseX, 0);
        }

        // WASD로 이동
        Vector3 move = Vector3.zero;
        if (Input.GetKey(KeyCode.W)) move += transform.forward;
        if (Input.GetKey(KeyCode.S)) move -= transform.forward;
        if (Input.GetKey(KeyCode.A)) move -= transform.right;
        if (Input.GetKey(KeyCode.D)) move += transform.right;
        if (Input.GetKey(KeyCode.Q)) move += transform.up;
        if (Input.GetKey(KeyCode.Z)) move -= transform.up;

        transform.position += move * 10f * Time.deltaTime;
    }

    void UpdateCameraShake()
    {
        if (!enableCameraShake || carController == null) return;

        // 속도에 따른 카메라 쉐이크 (더 부드럽게)
        float speedRatio = Mathf.Clamp01(carController.CurrentSpeed / maxShakeSpeed);
        float baseShakeAmount = speedRatio * shakeIntensity;

        // 드리프트 시 추가 쉐이크 (덜 강하게)
        float driftMultiplier = carController.IsDrifting ? 1.5f : 1f;
        float shakeAmount = baseShakeAmount * driftMultiplier;

        // 부드러운 쉐이크를 위한 시간 기반 노이즈
        float time = Time.time * shakeSmoothing;
        Vector3 targetShake = new Vector3(
            Mathf.PerlinNoise(time, 0f) * 2f - 1f,      // -1 ~ 1 범위
            Mathf.PerlinNoise(0f, time) * 2f - 1f,      // -1 ~ 1 범위
            Mathf.PerlinNoise(time, time) * 2f - 1f     // -1 ~ 1 범위
        ) * shakeAmount;

        // 부드럽게 쉐이크 적용
        shakeOffset = Vector3.Lerp(shakeOffset, targetShake, 10f * Time.deltaTime);

        // 위치에 쉐이크 적용
        transform.position += shakeOffset;
    }

    void UpdateFieldOfView()
    {
        if (carController == null) return;

        // 속도에 따른 FOV 조정 (스피드감 연출)
        float speedRatio = carController.CurrentSpeed / 200f;
        float targetFOV = originalFOV + (speedRatio * 20f); // 최대 20도 증가

        cameraComponent.fieldOfView = Mathf.Lerp(cameraComponent.fieldOfView, targetFOV, 2f * Time.deltaTime);
    }

    void AutoSwitchCamera()
    {
        if (!autoSwitchCamera || carController == null) return;

        // 고속 주행 시 자동으로 3인칭으로 전환
        if (carController.CurrentSpeed > highSpeedThreshold && currentMode == CameraMode.FirstPerson)
        {
            if (Time.time - lastSwitchTime > 2f) // 2초 쿨타임
            {
                SetCameraMode(CameraMode.ThirdPerson);
                lastSwitchTime = Time.time;
            }
        }
    }

    public void SetCameraMode(CameraMode mode)
    {
        currentMode = mode;

        // 모드별 초기 설정
        switch (mode)
        {
            case CameraMode.FirstPerson:
                cameraComponent.fieldOfView = originalFOV - 10f;
                break;
            case CameraMode.ThirdPerson:
                cameraComponent.fieldOfView = originalFOV;
                break;
            case CameraMode.TopView:
                cameraComponent.fieldOfView = originalFOV + 20f;
                break;
            case CameraMode.Cinematic:
                cameraComponent.fieldOfView = originalFOV + 10f;
                break;
            case CameraMode.Free:
                cameraComponent.fieldOfView = originalFOV;
                break;
        }
    }

    public void SwitchToNextCamera()
    {
        int nextMode = ((int)currentMode + 1) % System.Enum.GetValues(typeof(CameraMode)).Length;
        SetCameraMode((CameraMode)nextMode);

        // UI에 현재 카메라 모드 표시 (선택사항)
        Debug.Log($"카메라 모드: {currentMode}");
    }

    // 특정 위치를 바라보는 기능
    public void LookAt(Vector3 target, float duration = 1f)
    {
        StartCoroutine(LookAtCoroutine(target, duration));
    }

    private System.Collections.IEnumerator LookAtCoroutine(Vector3 target, float duration)
    {
        CameraMode originalMode = currentMode;
        currentMode = CameraMode.Free;

        Quaternion startRotation = transform.rotation;
        Vector3 direction = target - transform.position;
        Quaternion targetRotation = Quaternion.LookRotation(direction);

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            transform.rotation = Quaternion.Slerp(startRotation, targetRotation, t);
            yield return null;
        }

        currentMode = originalMode;
    }

    // 기즈모로 카메라 위치 표시
    void OnDrawGizmosSelected()
    {
        if (carTransform == null) return;

        Gizmos.color = Color.yellow;

        // 3인칭 카메라 위치
        Vector3 thirdPersonPos = carTransform.TransformPoint(thirdPersonOffset);
        Gizmos.DrawWireSphere(thirdPersonPos, 0.5f);
        Gizmos.DrawLine(carTransform.position, thirdPersonPos);

        // 1인칭 카메라 위치
        Gizmos.color = Color.red;
        Vector3 firstPersonPos = carTransform.TransformPoint(firstPersonOffset);
        Gizmos.DrawWireSphere(firstPersonPos, 0.2f);

        // 탑뷰 카메라 위치
        Gizmos.color = Color.blue;
        Vector3 topViewPos = carTransform.position + topViewOffset;
        Gizmos.DrawWireSphere(topViewPos, 1f);
        Gizmos.DrawLine(carTransform.position, topViewPos);

        // 🆕 커스텀 카메라 경로 표시 (부드러운 곡선으로 개선)
        if (useCustomCameraMoving && cameraWaypoints != null && cameraWaypoints.Length > 0)
        {
            Gizmos.color = Color.green;

            // 경로 포인트들 표시
            for (int i = 0; i < cameraWaypoints.Length; i++)
            {
                if (cameraWaypoints[i] != null)
                {
                    // 포인트 크기를 인덱스에 따라 다르게 (시작점이 더 크게)
                    float sphereSize = (i == 0) ? 1.2f : (i == cameraWaypoints.Length - 1) ? 1.0f : 0.8f;
                    Gizmos.DrawWireSphere(cameraWaypoints[i].position, sphereSize);

                    // 포인트 번호 표시를 위한 색상 변경
                    if (i == 0) Gizmos.color = Color.cyan; // 시작점
                    else if (i == cameraWaypoints.Length - 1) Gizmos.color = Color.red; // 끝점
                    else Gizmos.color = Color.green; // 중간점

                    Gizmos.DrawWireSphere(cameraWaypoints[i].position, sphereSize);
                    Gizmos.color = Color.green;
                }
            }

            // 🆕 부드러운 곡선 경로 표시 (여러 구간으로 나누어 표시)
            if (cameraWaypoints.Length >= 2)
            {
                Gizmos.color = Color.yellow;
                int pathResolution = 50; // 경로 해상도

                for (int i = 0; i < pathResolution; i++)
                {
                    float t1 = (float)i / pathResolution;
                    float t2 = (float)(i + 1) / pathResolution;

                    Vector3 point1 = GetPositionAlongPath(t1);
                    Vector3 point2 = GetPositionAlongPath(t2);

                    Gizmos.DrawLine(point1, point2);
                }
            }

            // 플레이어로의 시선 표시 (중요 포인트에만)
            if (lookAtPlayerDuringMoving && carTransform != null)
            {
                Gizmos.color = Color.cyan;
                for (int i = 0; i < cameraWaypoints.Length; i += 2) // 2개마다 하나씩만
                {
                    if (cameraWaypoints[i] != null)
                    {
                        Gizmos.DrawLine(cameraWaypoints[i].position, carTransform.position);
                    }
                }
            }
        }
    }

    public void SkipCustomCameraMoving()
    {
        if (customMovingCoroutine != null)
        {
            StopCoroutine(customMovingCoroutine);
            customMovingCoroutine = null;
        }
        isCustomMoving = false;
        SetCameraMode(CameraMode.ThirdPerson);

        // 이벤트 콜백도 호출 (게임 시작 등)
        OnCustomCameraMovingComplete?.Invoke();

        Debug.Log("✨ 커스텀 카메라 무빙 스킵됨! 일반 카메라 모드로 전환");
    }
}
