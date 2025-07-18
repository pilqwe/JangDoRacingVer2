using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CarUI : MonoBehaviour
{
    [Header("UI 참조")]
    public SportsCarController carController;
    public CarCameraController cameraController;

    [Header("속도계 UI")]
    public TextMeshProUGUI speedText;
    public TextMeshProUGUI gearText;
    public TextMeshProUGUI rpmText;
    public Image speedometerNeedle;           // 속도계 바늘
    public float maxSpeedometerAngle = 240f;  // 최대 회전각

    [Header("상태 표시")]
    public TextMeshProUGUI statusText;
    public Image driftIndicator;
    public Image brakeIndicator;
    public Image engineIndicator;

    [Header("카메라 UI")]
    public TextMeshProUGUI cameraInfoText;

    [Header("미니맵")]
    public Camera minimapCamera;
    public RawImage minimapDisplay;
    public Transform minimapCarIcon;

    [Header("성능 표시")]
    public Slider speedBar;
    public Slider rpmBar;
    public TextMeshProUGUI performanceText;

    [Header("컨트롤 가이드")]
    public GameObject controlGuidePanel;
    public bool showControlGuide = true;

    void Start()
    {
        // 자동으로 컴포넌트 찾기
        if (carController == null)
            carController = FindObjectOfType<SportsCarController>();

        if (cameraController == null)
            cameraController = FindObjectOfType<CarCameraController>();

        // 미니맵 설정
        SetupMinimap();

        // 컨트롤 가이드 표시
        if (controlGuidePanel != null)
            controlGuidePanel.SetActive(showControlGuide);
    }

    void Update()
    {
        if (carController == null) return;

        UpdateSpeedometer();
        UpdateGearDisplay();
        UpdateStatusIndicators();
        UpdateCameraInfo();
        UpdateMinimap();
        UpdatePerformanceMetrics();

        // UI 토글
        HandleUIInput();
    }

    void UpdateSpeedometer()
    {
        float currentSpeed = carController.CurrentSpeed;

        // 속도 텍스트 업데이트
        if (speedText != null)
        {
            speedText.text = $"{currentSpeed:F0} km/h";
        }

        // 속도계 바늘 회전
        if (speedometerNeedle != null)
        {
            float speedRatio = currentSpeed / carController.maxSpeed;
            float needleAngle = speedRatio * maxSpeedometerAngle;
            speedometerNeedle.transform.rotation = Quaternion.Euler(0, 0, -needleAngle);
        }

        // 속도 바 업데이트
        if (speedBar != null)
        {
            speedBar.value = currentSpeed / carController.maxSpeed;
        }
    }

    void UpdateGearDisplay()
    {
        // 기어 표시
        if (gearText != null)
        {
            string gearDisplay = carController.CurrentGear.ToString();
            if (carController.CurrentSpeed < 0)
                gearDisplay = "R"; // 후진

            gearText.text = $"기어: {gearDisplay}";

            // 기어 색상 변경
            if (carController.autoGear)
                gearText.color = Color.green;
            else
                gearText.color = Color.white;
        }

        // RPM 표시
        if (rpmText != null)
        {
            rpmText.text = $"RPM: {carController.RPM:F0}";
        }

        // RPM 바 업데이트
        if (rpmBar != null)
        {
            rpmBar.value = carController.RPM / 4000f; // 4000 RPM을 최대로 가정

            // RPM에 따른 색상 변경
            Image rpmBarFill = rpmBar.fillRect.GetComponent<Image>();
            if (rpmBarFill != null)
            {
                if (carController.RPM > 3500f)
                    rpmBarFill.color = Color.red;
                else if (carController.RPM > 2500f)
                    rpmBarFill.color = Color.yellow;
                else
                    rpmBarFill.color = Color.green;
            }
        }
    }

    void UpdateStatusIndicators()
    {
        // 드리프트 표시
        if (driftIndicator != null)
        {
            driftIndicator.color = carController.IsDrifting ?
                new Color(1f, 0.5f, 0f, 1f) : new Color(1f, 1f, 1f, 0.3f);
        }

        // 브레이크 표시
        if (brakeIndicator != null)
        {
            brakeIndicator.color = carController.IsBraking ?
                Color.red : new Color(1f, 1f, 1f, 0.3f);
        }

        // 엔진 상태 표시
        if (engineIndicator != null)
        {
            bool engineRunning = Mathf.Abs(Input.GetAxis("Vertical")) > 0.1f;
            engineIndicator.color = engineRunning ?
                Color.green : new Color(1f, 1f, 1f, 0.5f);
        }

        // 상태 텍스트 업데이트
        if (statusText != null)
        {
            string status = "";

            if (carController.IsDrifting)
                status += "드리프트 중! ";

            if (carController.IsBraking)
                status += "브레이크 ";

            if (!carController.IsGrounded)
                status += "공중에 떠있음! ";

            if (carController.CurrentSpeed > carController.maxSpeed * 0.9f)
                status += "최고속도! ";

            if (string.IsNullOrEmpty(status))
                status = "정상 주행";

            statusText.text = status;
        }
    }

    void UpdateCameraInfo()
    {
        if (cameraInfoText != null && cameraController != null)
        {
            string cameraInfo = $"카메라: {cameraController.currentMode}";
            cameraInfo += "\n[C] 카메라 전환";
            cameraInfo += "\n[1-5] 특정 카메라";
            cameraInfoText.text = cameraInfo;
        }
    }

    void UpdateMinimap()
    {
        if (minimapCarIcon != null && carController != null)
        {
            // 미니맵에서 차량 아이콘 회전
            minimapCarIcon.rotation = Quaternion.Euler(0, 0, -carController.transform.eulerAngles.y);
        }
    }

    void UpdatePerformanceMetrics()
    {
        if (performanceText != null)
        {
            string performance = $"FPS: {(1f / Time.deltaTime):F0}\n";
            performance += $"속도: {carController.CurrentSpeed:F1} km/h\n";
            performance += $"기어: {carController.CurrentGear}\n";
            performance += $"RPM: {carController.RPM:F0}\n";
            performance += $"지면 접촉: {(carController.IsGrounded ? "예" : "아니오")}\n";
            performance += $"드리프트: {(carController.IsDrifting ? "예" : "아니오")}";

            performanceText.text = performance;
        }
    }

    void HandleUIInput()
    {
        // F1 키로 컨트롤 가이드 토글
        if (Input.GetKeyDown(KeyCode.F1))
        {
            if (controlGuidePanel != null)
            {
                bool isActive = controlGuidePanel.activeSelf;
                controlGuidePanel.SetActive(!isActive);
            }
        }

        // Tab 키로 성능 정보 토글
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (performanceText != null)
            {
                performanceText.gameObject.SetActive(!performanceText.gameObject.activeSelf);
            }
        }
    }

    void SetupMinimap()
    {
        if (minimapCamera == null) return;

        // 미니맵 카메라 설정
        minimapCamera.orthographic = true;
        minimapCamera.orthographicSize = 50f;
        minimapCamera.transform.rotation = Quaternion.Euler(90f, 0f, 0f);

        // 미니맵 카메라가 차량을 따라가도록
        if (carController != null)
        {
            minimapCamera.transform.SetParent(carController.transform);
            minimapCamera.transform.localPosition = new Vector3(0, 100f, 0);
        }
    }

    // 외부에서 호출할 수 있는 메서드들
    public void ShowMessage(string message, float duration = 3f)
    {
        if (statusText != null)
        {
            StartCoroutine(ShowMessageCoroutine(message, duration));
        }
    }

    private System.Collections.IEnumerator ShowMessageCoroutine(string message, float duration)
    {
        string originalText = statusText.text;
        statusText.text = message;
        statusText.color = Color.yellow;

        yield return new WaitForSeconds(duration);

        statusText.text = originalText;
        statusText.color = Color.white;
    }

    public void SetUIVisibility(bool visible)
    {
        // 모든 UI 요소 표시/숨김
        Canvas canvas = GetComponent<Canvas>();
        if (canvas != null)
        {
            canvas.enabled = visible;
        }
    }

    // 랩타임 기록 (선택사항)
    public void RecordLapTime(float lapTime)
    {
        string lapTimeText = $"랩타임: {lapTime:F2}초";
        ShowMessage(lapTimeText, 5f);
    }
}
