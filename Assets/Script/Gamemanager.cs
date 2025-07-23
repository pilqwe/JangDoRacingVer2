using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;
using UnityEngine.Splines;
using System.Collections.Generic;


public class RacingGameManager : MonoBehaviour
{
    [Header("게임 시작 UI")]
    public GameObject startBackgroundPanel;  // 🆕 게임 시작 배경 패널
    public GameObject startPanel;            // 시작 버튼 UI 패널
    public Button startButton;               // 시작 버튼
    
    [Header("게임 진행 UI")]
    public GameObject restartPanel;          // 재시작 버튼 UI 패널
    public Button restartButton;             // 재시작 버튼
    public GameObject pausePanel;            // 일시정지 패널 (계속/재시작 버튼 포함)
    public Button resumeButton;              // 계속 버튼
    public Button pauseRestartButton;        // 일시정지 내 재시작 버튼
    public TMPro.TextMeshProUGUI countdownText; // 카운트다운 텍스트 (TextMeshPro)
    
    [Header("게임 오브젝트")]
    public GameObject player;                // 플레이어 오브젝트
    public SplineBotController[] bots;       // 여러 봇을 Inspector에서 연결
    public int totalLaps = 3;                // 총 랩 수
    public MeshRenderer[] startLights;       // Inspector에서 3개의 라이트 오브젝트 연결
    public SplineAnimate splineAnimator;     // 스플라인 애니메이터 (Inspector에서 연결)
    public CarCameraController cameraController; // 🆕 카메라 컨트롤러

    [Header("배경 전환 효과")]
    public float backgroundFadeOutTime = 1.0f; // 배경 사라지는 시간

    private int currentLap = 0;
    private bool raceStarted = false;
    private bool isPaused = false;

    private List<float> lapTimes = new List<float>();
    private float lapStartTime = 0f;
    private bool lapStarted = false; // 랩 타이머 시작 여부

    public static RacingGameManager Instance; // 싱글톤

    void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        // ESC 키로 일시정지
        if (raceStarted && !isPaused && Input.GetKeyDown(KeyCode.Escape))
        {
            PauseGame();
        }
    }

    void Start()
    {
        // 🆕 게임 시작 시 배경과 시작 패널 모두 표시
        if (startBackgroundPanel != null)
            startBackgroundPanel.SetActive(true);
        
        startPanel.SetActive(true);
        countdownText.gameObject.SetActive(false);
        restartPanel.SetActive(false);
        pausePanel.SetActive(false);
        
        // 버튼 이벤트 연결
        startButton.onClick.AddListener(OnStartButton);
        restartButton.onClick.AddListener(OnRestartButton);
        resumeButton.onClick.AddListener(OnResumeButton);
        pauseRestartButton.onClick.AddListener(OnRestartButton);
        
        // 봇들 비활성화
        foreach (var bot in bots)
            bot.enabled = false;
            
        // 🆕 카메라 컨트롤러 자동 찾기 (Inspector에서 할당하지 않은 경우)
        if (cameraController == null)
            cameraController = FindObjectOfType<CarCameraController>();
            
        // 초기화
        lapTimes.Clear();
        lapStartTime = Time.time;
        
        Debug.Log("🎮 게임 시작 화면 준비 완료!");
    }

    public void OnStartButton()
    {
        Debug.Log("🚀 시작 버튼 클릭됨!");
        
        if (startBackgroundPanel != null)
        {
            // 배경이 있을 경우 페이드 아웃 후 게임 시작
            StartCoroutine(StartGameSequence());
        }
        else
        {
            // 배경이 없을 경우 바로 게임 시작
            StartGameDirect();
        }
    }
    
    /// <summary>
    /// 🎬 배경 페이드 아웃과 함께 게임 시작 시퀀스
    /// </summary>
    IEnumerator StartGameSequence()
    {
        Debug.Log("🌟 게임 시작 시퀀스 시작!");
        
        // 배경 페이드 아웃
        yield return StartCoroutine(FadeOutBackground());
        
        // 게임 직접 시작
        StartGameDirect();
    }
    
    /// <summary>
    /// 🌘 배경 패널 페이드 아웃 코루틴
    /// </summary>
    IEnumerator FadeOutBackground()
    {
        Debug.Log("🌫️ 배경 페이드 아웃 시작!");
        
        CanvasGroup backgroundCanvasGroup = startBackgroundPanel.GetComponent<CanvasGroup>();
        
        // CanvasGroup이 없으면 추가
        if (backgroundCanvasGroup == null)
        {
            backgroundCanvasGroup = startBackgroundPanel.AddComponent<CanvasGroup>();
            Debug.Log("📝 CanvasGroup 컴포넌트 자동 추가됨!");
        }
        
        float elapsedTime = 0f;
        float startAlpha = backgroundCanvasGroup.alpha;
        
        while (elapsedTime < backgroundFadeOutTime)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / backgroundFadeOutTime;
            
            // 부드러운 페이드 아웃
            backgroundCanvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, progress);
            
            yield return null;
        }
        
        // 완전히 투명하게 하고 비활성화
        backgroundCanvasGroup.alpha = 0f;
        startBackgroundPanel.SetActive(false);
        
        Debug.Log("✨ 배경 페이드 아웃 완료!");
    }
    
    /// <summary>
    /// 🏁 게임 직접 시작 (기존 로직)
    /// </summary>
    void StartGameDirect()
    {
        Debug.Log("🎯 게임 직접 시작!");
        
        startPanel.SetActive(false);
        
        // 🆕 카메라 무빙을 카운트다운 전에 먼저 시작
        if (cameraController != null)
        {
            // 🆕 커스텀 카메라 무빙이 있는 경우 완료 후 카운트다운 시작
            if (cameraController.useCustomCameraMoving && cameraController.cameraWaypoints != null && cameraController.cameraWaypoints.Length > 0)
            {
                // 카메라 무빙 완료 이벤트에 카운트다운 시작 연결
                cameraController.OnCustomCameraMovingComplete = OnCustomCameraMovingComplete;
                cameraController.StartGameCamera();
                Debug.Log("📹 커스텀 카메라 무빙 시작! 완료 후 카운트다운 예정");
            }
            else
            {
                // 기본 카메라인 경우 바로 카운트다운 시작
                cameraController.StartGameCamera();
                StartCoroutine(StartCountdown());
                Debug.Log("📹 기본 카메라 무빙 시작 + 카운트다운 시작!");
            }
        }
        else
        {
            Debug.LogWarning("⚠️ 카메라 컨트롤러가 null입니다!");
            StartCoroutine(StartCountdown());
        }
    }
    
    /// <summary>
    /// 🎬 커스텀 카메라 무빙 완료 후 호출되는 콜백
    /// </summary>
    void OnCustomCameraMovingComplete()
    {
        Debug.Log("🎬 커스텀 카메라 무빙 완료! 이제 카운트다운 시작");
        StartCoroutine(StartCountdown());
    }

    IEnumerator StartCountdown()
    {
        countdownText.gameObject.SetActive(true);
        // 모든 라이트 꺼두기
        foreach (var light in startLights)
            light.enabled = false;

        string[] counts = { "3", "2", "1", "Go!" };
        for (int i = 0; i < counts.Length; i++)
        {
            countdownText.text = counts[i];
            Debug.Log($"카운트다운: {counts[i]}");

            // 3,2,1에 맞춰 라이트 켜기
            if (i < startLights.Length)
                startLights[i].enabled = true;

            yield return new WaitForSeconds(1f);
        }
        countdownText.gameObject.SetActive(false);
        Debug.Log("카운트다운 텍스트 비활성화됨");

        // 모든 라이트 끄기 (혹은 Go!에 맞춰 연출)
        foreach (var light in startLights)
            light.enabled = false;

        StartRace();
    }

    void StartRace()
    {
        // UIManager 존재 확인
        if (UIManager.Instance != null)
        {
            UIManager.Instance.StartRaceTimer();
        }
        else
        {
            Debug.LogWarning("⚠️ UIManager.Instance가 null입니다!");
        }
        
        // 스플라인 애니메이터 시작
        if (splineAnimator != null)
        {
            splineAnimator.Play(); 
            Debug.Log("🎯 스플라인 애니메이션 시작!");
        }
        else
        {
            Debug.LogWarning("⚠️ splineAnimator가 null입니다!");
        }
        
        raceStarted = true;
        currentLap = 1;
        
        // 플레이어 활성화
        if (player != null)
        {
            player.SetActive(true);
            Debug.Log("🎮 플레이어 활성화!");
        }
        
        // 봇들 활성화
        foreach (var bot in bots)
            bot.enabled = true;
            
        Debug.Log("🚀 레이스 시작 완료!");
    }

    // 랩 트리거에서 호출
    public void OnLapTrigger()
    {
        if (!lapStarted)
        {
            // 첫 랩 트리거 통과 시 타이머 시작 (Lap 1)
            lapStarted = true;
            lapStartTime = Time.time;
            currentLap = 1;
            UIManager.Instance.UpdateLapUI(currentLap, totalLaps);
            return;
        }

        // 이후부터 랩 타임 기록
        float lapTime = Time.time - lapStartTime;
        lapTimes.Add(lapTime);
        lapStartTime = Time.time;

        currentLap++;
        if (currentLap > totalLaps)
        {
            EndRace();
        }
        else
        {
            UIManager.Instance.UpdateLapUI(currentLap, totalLaps);
            UIManager.Instance.UpdateLapTimeList(lapTimes);
        }
    }

    public List<float> GetLapTimes()
    {
        return lapTimes;
    }

    void EndRace()
    {
        raceStarted = false;
        
        // 🆕 카메라 무빙 중단
        if (cameraController != null)
        {
            cameraController.StopGameCamera();
            Debug.Log("📹 카메라 무빙 중단!");
        }
        
        foreach (var bot in bots)
            bot.enabled = false;
            
        Debug.Log("레이스 종료!");
        restartPanel.SetActive(true);
    }

    void OnRestartButton()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
    }

    void PauseGame()
    {
        isPaused = true;
        pausePanel.SetActive(true);
        Time.timeScale = 0f;
    }

    void OnResumeButton()
    {
        isPaused = false;
        pausePanel.SetActive(false);
        Time.timeScale = 1f;
    }
}
