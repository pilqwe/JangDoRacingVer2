using UnityEngine;

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

    // 프라이빗 변수들
    private Camera cameraComponent;
    private Vector3 currentVelocity;
    private Vector3 targetPosition;
    private Vector3 lookAheadTarget;
    private float originalFOV;
    private Vector3 shakeOffset;
    private float lastSwitchTime;

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
    }

    void LateUpdate()
    {
        if (carTransform == null) return;

        HandleInput();
        UpdateCameraPosition();
        UpdateCameraShake();
        UpdateFieldOfView();
        AutoSwitchCamera();
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
    }
}
