using UnityEngine;

public class SportsCarController : MonoBehaviour
{
    [Header("스포츠카 엔진 설정")]
    public float maxSpeed = 120f;              // 최대 속도 (km/h) - 대폭 감소
    public float acceleration = 12f;           // 가속도 - 절반으로 감소
    public float brakeForce = 80f;             // 브레이크 힘 - 증가
    public float reverseSpeed = 25f;           // 후진 최대 속도 - 절반으로 감소

    [Header("조향 설정")]
    public float maxSteerAngle = 30f;          // 최대 조향각
    public float steerSensitivity = 1.5f;      // 조향 민감도
    public float returnSpeed = 10f;            // 조향 복귀 속도

    [Header("물리 설정")]
    public float downforce = 5000f;            // 다운포스 (더 강하게)
    public float centerOfMass = -2f;           // 무게중심 (더 낮게)
    public float antiRoll = 12000f;            // 안티롤바 효과 (더 강하게)
    public float customGravity = 50f;          // 추가 중력 (더 강하게)
    public bool useCustomPhysics = true;       // 커스텀 물리 사용

    [Header("드리프트 설정")]
    public float driftFactor = 0.85f;          // 드리프트 정도 (더 적게)
    public float gripFactor = 8f;              // 그립력 (더 강하게)
    public float driftThreshold = 0.6f;        // 드리프트 시작 임계값 (더 높게)

    [Header("기어 시스템")]
    public int numberOfGears = 6;              // 기어 수
    public float[] gearRatios = { 3.5f, 2.5f, 1.8f, 1.3f, 1.0f, 0.8f }; // 기어비
    public bool autoGear = true;               // 자동 기어

    [Header("사운드")]
    public AudioSource engineSound;            // 엔진 소리
    public AudioSource tireSquealSound;        // 타이어 끼익 소리
    public AudioSource windSound;              // 바람 소리

    [Header("비주얼 효과")]
    public ParticleSystem[] exhaustParticles;  // 배기가스 파티클
    public ParticleSystem[] tireSmoke;         // 타이어 연기
    public Light[] headlights;                 // 헤드라이트
    public Light[] brakelights;                // 브레이크등

    // 휠 컬라이더 (인스펙터에서 할당)
    [Header("휠 컬라이더")]
    public WheelCollider frontLeftWheel;
    public WheelCollider frontRightWheel;
    public WheelCollider rearLeftWheel;
    public WheelCollider rearRightWheel;

    [Header("휠 메시 (선택사항)")]
    public Transform frontLeftWheelMesh;
    public Transform frontRightWheelMesh;
    public Transform rearLeftWheelMesh;
    public Transform rearRightWheelMesh;

    // 프라이빗 변수들
    private Rigidbody carRigidbody;
    private float motorInput;
    private float steerInput;
    private float brakeInput;
    private bool handbrakeInput;
    private float currentSpeed;
    private int currentGear = 1;
    private float rpm;
    private bool isDrifting;
    private bool isGrounded;

    // UI 정보용 프로퍼티
    public float CurrentSpeed => currentSpeed;
    public int CurrentGear => currentGear;
    public float RPM => rpm;
    public bool IsDrifting => isDrifting;
    public bool IsGrounded => isGrounded;
    public bool IsBraking => brakeInput > 0.1f || handbrakeInput;

    void Start()
    {
        carRigidbody = GetComponent<Rigidbody>();

        // Rigidbody가 없으면 추가
        if (carRigidbody == null)
        {
            carRigidbody = gameObject.AddComponent<Rigidbody>();
            carRigidbody.mass = 3500f; // 스포츠카 무게 (더 무겁게)
            carRigidbody.linearDamping = 1f; // 공기 저항 대폭 증가
            carRigidbody.angularDamping = 15f;  // 회전 감쇠 대폭 증가
        }

        // 무게중심 설정
        carRigidbody.centerOfMass = new Vector3(0, centerOfMass, 0);

        // 휠 컬라이더가 없으면 자동 생성
        SetupWheelColliders();

        // 헤드라이트 켜기
        SetHeadlights(true);
    }

    void Update()
    {
        HandleInput();
        UpdateWheelMeshes();
        HandleAudio();
        HandleVisualEffects();

        // 현재 속도 계산 (km/h)
        currentSpeed = carRigidbody.linearVelocity.magnitude * 3.6f;

        // 지면 접촉 체크
        CheckGrounded();

        // 드리프트 감지
        DetectDrift();

        // 자동 기어
        if (autoGear)
        {
            AutoGearShift();
        }
    }

    void FixedUpdate()
    {
        HandleMotor();
        HandleSteering();
        HandleBraking();
        ApplyDownforce();
        ApplyAntiRoll();
        ApplyCustomGravity();
    }

    void HandleInput()
    {
        motorInput = Input.GetAxis("Vertical");
        steerInput = Input.GetAxis("Horizontal");
        brakeInput = Input.GetKey(KeyCode.Space) ? 1f : 0f;
        handbrakeInput = Input.GetKey(KeyCode.LeftShift);

        // 수동 기어 조작 (선택사항)
        if (!autoGear)
        {
            if (Input.GetKeyDown(KeyCode.Q) && currentGear > 1)
                currentGear--;
            if (Input.GetKeyDown(KeyCode.E) && currentGear < numberOfGears)
                currentGear++;
        }

        // 헤드라이트 토글
        if (Input.GetKeyDown(KeyCode.L))
        {
            ToggleHeadlights();
        }

        // 리셋
        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetCar();
        }
    }

    void HandleMotor()
    {
        float motor = motorInput * acceleration * 800f;  // 토크 대폭 감소 (1500 → 800)

        // 기어비 적용
        if (currentGear > 0 && currentGear <= gearRatios.Length)
        {
            motor *= gearRatios[currentGear - 1];
        }

        // 후진 처리
        if (motorInput < 0 && currentSpeed < 5f)
        {
            motor = motorInput * reverseSpeed * 800f;  // 후진 토크도 감소
        }

        // 뒷바퀴에 동력 전달 (RWD)
        rearLeftWheel.motorTorque = motor;
        rearRightWheel.motorTorque = motor;

        // RPM 계산
        rpm = Mathf.Abs(rearLeftWheel.rpm + rearRightWheel.rpm) / 2f;
    }

    void HandleSteering()
    {
        float steer = steerInput * maxSteerAngle * steerSensitivity;

        // 속도에 따른 조향 보정 (더 강하게)
        float speedFactor = Mathf.Clamp01(1f - (currentSpeed / 80f));  // 더 낮은 속도부터 조향 제한
        steer *= speedFactor;

        frontLeftWheel.steerAngle = steer;
        frontRightWheel.steerAngle = steer;
    }

    void HandleBraking()
    {
        float brake = brakeInput * brakeForce * 5000f;  // 브레이크 토크 증가

        // 핸드브레이크 (뒷바퀴만)
        if (handbrakeInput)
        {
            rearLeftWheel.brakeTorque = brakeForce * 4000f;  // 핸드브레이크 토크 증가
            rearRightWheel.brakeTorque = brakeForce * 4000f;
            frontLeftWheel.brakeTorque = 0f;
            frontRightWheel.brakeTorque = 0f;
        }
        else
        {
            // 일반 브레이크 (모든 바퀴)
            frontLeftWheel.brakeTorque = brake;
            frontRightWheel.brakeTorque = brake;
            rearLeftWheel.brakeTorque = brake * 0.8f; // 뒷바퀴 브레이크 증가
            rearRightWheel.brakeTorque = brake * 0.8f;
        }

        // 브레이크등 제어
        SetBrakelights(brakeInput > 0.1f || handbrakeInput);
    }

    void ApplyDownforce()
    {
        // 속도에 비례한 다운포스 적용
        float speedFactor = currentSpeed / 100f;
        Vector3 downforceVector = -transform.up * downforce * speedFactor * speedFactor;
        carRigidbody.AddForce(downforceVector);
    }

    void ApplyAntiRoll()
    {
        // 안티롤바 효과로 롤링 방지
        WheelCollider leftWheel = frontLeftWheel;
        WheelCollider rightWheel = frontRightWheel;

        WheelHit hit;
        float travelL = 1.0f;
        float travelR = 1.0f;

        bool groundedL = leftWheel.GetGroundHit(out hit);
        if (groundedL)
            travelL = (-leftWheel.transform.InverseTransformPoint(hit.point).y - leftWheel.radius) / leftWheel.suspensionDistance;

        bool groundedR = rightWheel.GetGroundHit(out hit);
        if (groundedR)
            travelR = (-rightWheel.transform.InverseTransformPoint(hit.point).y - rightWheel.radius) / rightWheel.suspensionDistance;

        float antiRollForce = (travelL - travelR) * antiRoll;

        if (groundedL)
            carRigidbody.AddForceAtPosition(leftWheel.transform.up * -antiRollForce, leftWheel.transform.position);
        if (groundedR)
            carRigidbody.AddForceAtPosition(rightWheel.transform.up * antiRollForce, rightWheel.transform.position);
    }

    void CheckGrounded()
    {
        isGrounded = frontLeftWheel.isGrounded || frontRightWheel.isGrounded ||
                     rearLeftWheel.isGrounded || rearRightWheel.isGrounded;
    }

    void DetectDrift()
    {
        // 속도 벡터와 전방 벡터의 각도 차이로 드리프트 감지
        Vector3 velocityDirection = carRigidbody.linearVelocity.normalized;
        float angle = Vector3.Angle(transform.forward, velocityDirection);

        isDrifting = angle > driftThreshold * 90f && currentSpeed > 20f;
    }

    void AutoGearShift()
    {
        if (rpm > 3000f && currentGear < numberOfGears)
        {
            currentGear++;
        }
        else if (rpm < 1500f && currentGear > 1)
        {
            currentGear--;
        }
    }

    void ApplyCustomGravity()
    {
        if (!useCustomPhysics) return;

        // 추가 중력 적용 (차가 더 무겁게 느껴지도록)
        Vector3 additionalGravity = Vector3.down * customGravity;
        carRigidbody.AddForce(additionalGravity, ForceMode.Acceleration);

        // 지면에 있을 때는 안정성을 위해 약간의 다운포스 추가
        if (isGrounded)
        {
            Vector3 groundStick = -transform.up * (customGravity * 0.5f);
            carRigidbody.AddForce(groundStick, ForceMode.Acceleration);
        }
    }

    void UpdateWheelMeshes()
    {
        UpdateWheelMesh(frontLeftWheel, frontLeftWheelMesh);
        UpdateWheelMesh(frontRightWheel, frontRightWheelMesh);
        UpdateWheelMesh(rearLeftWheel, rearLeftWheelMesh);
        UpdateWheelMesh(rearRightWheel, rearRightWheelMesh);
    }

    void UpdateWheelMesh(WheelCollider wheelCollider, Transform wheelMesh)
    {
        if (wheelMesh != null)
        {
            Vector3 pos;
            Quaternion rot;
            wheelCollider.GetWorldPose(out pos, out rot);
            wheelMesh.position = pos;
            wheelMesh.rotation = rot;
        }
    }

    void HandleAudio()
    {
        // 엔진 사운드
        if (engineSound != null)
        {
            float pitch = Mathf.Lerp(0.8f, 2.5f, rpm / 4000f);
            engineSound.pitch = pitch;
            engineSound.volume = Mathf.Lerp(0.4f, 1.0f, Mathf.Abs(motorInput));
        }

        // 타이어 끼익 소리
        if (tireSquealSound != null)
        {
            if (isDrifting || handbrakeInput)
            {
                if (!tireSquealSound.isPlaying)
                    tireSquealSound.Play();
                tireSquealSound.volume = Mathf.Lerp(0f, 0.8f, currentSpeed / 50f);
            }
            else
            {
                tireSquealSound.Stop();
            }
        }

        // 바람 소리
        if (windSound != null)
        {
            windSound.volume = Mathf.Lerp(0f, 0.6f, currentSpeed / 150f);
            if (currentSpeed > 30f && !windSound.isPlaying)
                windSound.Play();
            else if (currentSpeed <= 30f && windSound.isPlaying)
                windSound.Stop();
        }
    }

    void HandleVisualEffects()
    {
        // 배기가스 파티클
        if (exhaustParticles != null)
        {
            foreach (var exhaust in exhaustParticles)
            {
                if (exhaust != null)
                {
                    var emission = exhaust.emission;
                    emission.rateOverTime = Mathf.Lerp(5f, 50f, Mathf.Abs(motorInput));
                }
            }
        }

        // 타이어 연기
        if (tireSmoke != null)
        {
            foreach (var smoke in tireSmoke)
            {
                if (smoke != null)
                {
                    if (isDrifting || handbrakeInput)
                    {
                        if (!smoke.isPlaying) smoke.Play();
                    }
                    else
                    {
                        if (smoke.isPlaying) smoke.Stop();
                    }
                }
            }
        }
    }

    void SetHeadlights(bool state)
    {
        if (headlights != null)
        {
            foreach (var light in headlights)
            {
                if (light != null) light.enabled = state;
            }
        }
    }

    void ToggleHeadlights()
    {
        if (headlights != null && headlights.Length > 0)
        {
            bool currentState = headlights[0].enabled;
            SetHeadlights(!currentState);
        }
    }

    void SetBrakelights(bool state)
    {
        if (brakelights != null)
        {
            foreach (var light in brakelights)
            {
                if (light != null) light.enabled = state;
            }
        }
    }

    void ResetCar()
    {
        transform.position = transform.position + Vector3.up * 2f;
        transform.rotation = Quaternion.Euler(0, transform.eulerAngles.y, 0);
        carRigidbody.linearVelocity = Vector3.zero;
        carRigidbody.angularVelocity = Vector3.zero;
        currentGear = 1;
    }

    void SetupWheelColliders()
    {
        // 기본 휠 컬라이더 설정
        WheelCollider[] wheels = { frontLeftWheel, frontRightWheel, rearLeftWheel, rearRightWheel };

        foreach (var wheel in wheels)
        {
            if (wheel != null)
            {
                JointSpring spring = wheel.suspensionSpring;
                spring.spring = 80000f;        // 서스펜션 매우 단단하게
                spring.damper = 10000f;        // 댐핑 매우 강하게
                spring.targetPosition = 0.2f;  // 더 낮은 자세
                wheel.suspensionSpring = spring;

                wheel.suspensionDistance = 0.2f;   // 서스펜션 거리 더 줄임
                wheel.radius = 0.4f;
                wheel.mass = 80f;                  // 휠 무게 더 증가

                WheelFrictionCurve forwardFriction = wheel.forwardFriction;
                WheelFrictionCurve sidewaysFriction = wheel.sidewaysFriction;

                forwardFriction.extremumSlip = 0.3f;      // 더 낮은 슬립
                forwardFriction.extremumValue = 1.2f;     // 더 강한 그립
                forwardFriction.asymptoteSlip = 0.6f;     // 더 낮은 슬립
                forwardFriction.asymptoteValue = 0.8f;    // 더 강한 그립
                forwardFriction.stiffness = gripFactor * 1.5f;  // 더 강한 강성

                sidewaysFriction.extremumSlip = 0.2f;     // 더 낮은 슬립
                sidewaysFriction.extremumValue = 1.2f;    // 더 강한 그립
                sidewaysFriction.asymptoteSlip = 0.4f;    // 더 낮은 슬립
                sidewaysFriction.asymptoteValue = 0.9f;   // 더 강한 그립
                sidewaysFriction.stiffness = gripFactor * driftFactor * 1.5f;  // 더 강한 강성

                wheel.forwardFriction = forwardFriction;
                wheel.sidewaysFriction = sidewaysFriction;
            }
        }
    }

    // 기즈모로 무게중심 표시
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(transform.position + transform.up * centerOfMass, 0.2f);

        Gizmos.color = isGrounded ? Color.green : Color.red;
        Gizmos.DrawRay(transform.position, Vector3.down * 2f);
    }
}
