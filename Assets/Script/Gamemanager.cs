using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;
using UnityEngine.Splines;
using System.Collections.Generic;


public class RacingGameManager : MonoBehaviour
{
    [Header("게임 �작 UI")]
    public GameObject startBackgroundPanel;  // �� 게임 �작 배경 �널
    public GameObject startPanel;            // �작 버튼 UI �널
    public Button startButton;               // �작 버튼
    
    [Header("게임 진행 UI")]
    public GameObject restartPanel;          // �시버튼 UI �널
    public Button restartButton;             // �시버튼
    public GameObject pausePanel;            // �시�� �널 (계속/�시버튼 �함)
    public Button resumeButton;              // 계속 버튼
    public Button pauseRestartButton;        // �시�� �시버튼
    public TMPro.TextMeshProUGUI countdownText; // 카운�다�스(TextMeshPro)
    
    [Header("게임 오브젝트")]
    public GameObject player;                // 플레이어 오브젝트
    public SplineBotController[] bots;       // 여러 봇을 Inspector에서 연결
    public int totalLaps = 3;                // 총 랩 수
    public MeshRenderer[] startLights;       // Inspector에서 3개의 라이트 오브젝트 연결
    public SplineAnimate splineAnimator;     // 스플라인 애니메이터 (Inspector에서 연결)
    public CarCameraController cameraController; // 🆕 카메라 컨트롤러

    [Header("배경 �환 �과")]
    public float backgroundFadeOutTime = 1.0f; // 배경 �라지�간

    private int currentLap = 0;
    private bool raceStarted = false;
    private bool isPaused = false;

    private List<float> lapTimes = new List<float>();
    private float lapStartTime = 0f;
    private bool lapStarted = false; // ��머 �작 ��

    public static RacingGameManager Instance; // ��
    void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        // ESC �로 �시��
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
        
        if (startPanel != null)
            startPanel.SetActive(true);
        else
            Debug.LogWarning("⚠️ startPanel이 Inspector에서 할당되지 않았습니다!");
            
        if (countdownText != null)
            countdownText.gameObject.SetActive(false);
        else
            Debug.LogWarning("⚠️ countdownText가 Inspector에서 할당되지 않았습니다!");
            
        if (restartPanel != null)
            restartPanel.SetActive(false);
        else
            Debug.LogWarning("⚠️ restartPanel이 Inspector에서 할당되지 않았습니다!");
            
        if (pausePanel != null)
            pausePanel.SetActive(false);
        else
            Debug.LogWarning("⚠️ pausePanel이 Inspector에서 할당되지 않았습니다!");
        
        // 버튼 이벤트 연결 (null 체크 추가)
        if (startButton != null)
            startButton.onClick.AddListener(OnStartButton);
        else
            Debug.LogWarning("⚠️ startButton이 Inspector에서 할당되지 않았습니다!");
            
        if (restartButton != null)
            restartButton.onClick.AddListener(OnRestartButton);
        else
            Debug.LogWarning("⚠️ restartButton이 Inspector에서 할당되지 않았습니다!");
            
        if (resumeButton != null)
            resumeButton.onClick.AddListener(OnResumeButton);
        else
            Debug.LogWarning("⚠️ resumeButton이 Inspector에서 할당되지 않았습니다!");
            
        if (pauseRestartButton != null)
            pauseRestartButton.onClick.AddListener(OnRestartButton);
        else
            Debug.LogWarning("⚠️ pauseRestartButton이 Inspector에서 할당되지 않았습니다!");
        
        // 봇들 비활성화 (null 체크 추가)
        if (bots != null)
        {
            foreach (var bot in bots)
            {
                if (bot != null)
                    bot.enabled = false;
            }
        }
        else
        {
            Debug.LogWarning("⚠️ bots 배열이 Inspector에서 할당되지 않았습니다!");
        }
            
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
        Debug.Log(" �작 버튼 �릭");
        
        if (startBackgroundPanel != null)
        {
            // 배경�을 경우 �이�웃 게임 �작
            StartCoroutine(StartGameSequence());
        }
        else
        {
            // 배경�을 경우 바로 게임 �작
            StartGameDirect();
        }
    }
    
    /// <summary>
    /// �� 배경 �이�웃골께 게임 �작 �퀀    /// </summary>
    IEnumerator StartGameSequence()
    {
        Debug.Log("�� 게임 �작 �퀀�작!");
        
        // 배경 �이�웃
        yield return StartCoroutine(FadeOutBackground());
        
        // 게임 직접 �작
        StartGameDirect();
    }
    
    /// <summary>
    /// �� 배경 �널 �이�웃 코루    /// </summary>
    IEnumerator FadeOutBackground()
    {
        Debug.Log("���배경 �이�웃 �작!");
        
        CanvasGroup backgroundCanvasGroup = startBackgroundPanel.GetComponent<CanvasGroup>();
        
        // CanvasGroup�으�추�
        if (backgroundCanvasGroup == null)
        {
            backgroundCanvasGroup = startBackgroundPanel.AddComponent<CanvasGroup>();
            Debug.Log("�� CanvasGroup 컴포�트 �동 추�");
        }
        
        float elapsedTime = 0f;
        float startAlpha = backgroundCanvasGroup.alpha;
        
        while (elapsedTime < backgroundFadeOutTime)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / backgroundFadeOutTime;
            
            // 부�러�이�웃
            backgroundCanvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, progress);
            
            yield return null;
        }
        
        // �전�명�게 �고 비활�화
        backgroundCanvasGroup.alpha = 0f;
        startBackgroundPanel.SetActive(false);
        
        Debug.Log("배경 �이�웃 �료!");
    }
    
    /// <summary>
    /// 🆕 게임 직접 시작 (기존 로직)
    /// </summary>
    void StartGameDirect()
    {
        Debug.Log("🎮 게임 직접 시작!");
        
        if (startPanel != null)
            startPanel.SetActive(false);
        
        // 🆕 카메라무빙과 카운트다운에 먼저 시작
        if (cameraController != null)
        {
            // 🆕 커스텀 카메라무빙이 있는 경우 완료 후 카운트다운 시작
            if (cameraController.useCustomCameraMoving && cameraController.cameraWaypoints != null && cameraController.cameraWaypoints.Length > 0)
            {
                // 카메라무빙 완료 이벤트에 카운트다운 시작 연결
                cameraController.OnCustomCameraMovingComplete = OnCustomCameraMovingComplete;
                cameraController.StartGameCamera();
                Debug.Log("🎬 커스텀 카메라무빙 시작! 완료 후 카운트다운 예정");
            }
            else
            {
                // 기본 카메라인 경우 바로 카운트다운 시작
                cameraController.StartGameCamera();
                StartCoroutine(StartCountdown());
                Debug.Log("📹 기본 카메라무빙 시작 + 카운트다운 시작!");
            }
        }
        else
        {
            Debug.LogWarning("⚠️ 카메라컨트롤러가 null입니다!");
            StartCoroutine(StartCountdown());
        }
    }
    
    /// <summary>
    /// 🆕 커스텀 카메라무빙 완료 후 호출되는 콜백
    /// </summary>
    void OnCustomCameraMovingComplete()
    {
        Debug.Log("🎬 커스텀 카메라무빙 완료! 이제 카운트다운 시작");
        StartCoroutine(StartCountdown());
    }

    IEnumerator StartCountdown()
    {
        Debug.Log("🔥 StartCountdown 시작!");
        
        // countdownText null 체크
        if (countdownText == null)
        {
            Debug.LogError("❌ countdownText가 null입니다! Inspector에서 할당해주세요.");
            yield break;
        }
        
        countdownText.gameObject.SetActive(true);
        Debug.Log("✅ countdownText 활성화됨");
        
        // 🔍 UI 상태 상세 디버깅
        Debug.Log($"🔍 countdownText 오브젝트 이름: {countdownText.gameObject.name}");
        Debug.Log($"🔍 countdownText 위치: {countdownText.transform.position}");
        Debug.Log($"🔍 countdownText 활성 상태: {countdownText.gameObject.activeInHierarchy}");
        Debug.Log($"🔍 countdownText 색상: {countdownText.color}");
        Debug.Log($"🔍 countdownText 폰트 크기: {countdownText.fontSize}");
        Debug.Log($"🔍 countdownText Canvas: {countdownText.canvas?.name ?? "null"}");
        
        // Canvas 상태 확인
        Canvas parentCanvas = countdownText.GetComponentInParent<Canvas>();
        if (parentCanvas != null)
        {
            Debug.Log($"🔍 부모 Canvas: {parentCanvas.name}, 활성 상태: {parentCanvas.gameObject.activeInHierarchy}");
            Debug.Log($"🔍 Canvas Render Mode: {parentCanvas.renderMode}");
        }
        else
        {
            Debug.LogError("❌ countdownText가 Canvas 하위에 없습니다!");
        }
        
        // 모든 라이트 꺼두기 (null 체크 추가)
        if (startLights != null)
        {
            foreach (var light in startLights)
            {
                if (light != null)
                    light.enabled = false;
            }
        }

        string[] counts = { "3", "2", "1", "Go!" };
        for (int i = 0; i < counts.Length; i++)
        {
            countdownText.text = counts[i];
            Debug.Log($"🔢 카운트다운: {counts[i]} (텍스트 설정됨)");
            
            // 🔍 텍스트 설정 후 상태 확인
            Debug.Log($"🔍 현재 텍스트: '{countdownText.text}'");
            Debug.Log($"🔍 텍스트 색상 알파값: {countdownText.color.a}");
            Debug.Log($"🔍 오브젝트 활성 상태: {countdownText.gameObject.activeInHierarchy}");

            // 3,2,1에 맞춰 라이트 켜기
            if (startLights != null && i < startLights.Length && startLights[i] != null)
                startLights[i].enabled = true;

            yield return new WaitForSeconds(1f);
        }
        countdownText.gameObject.SetActive(false);
        Debug.Log("✅ 카운트다운 텍스트 비활성화됨");

        // 모든 라이트 끄기 (혹은 Go!에 맞춰 연출)
        if (startLights != null)
        {
            foreach (var light in startLights)
            {
                if (light != null)
                    light.enabled = false;
            }
        }

        StartRace();
    }

    void StartRace()
    {
        // UIManager 존재 �인
        if (UIManager.Instance != null)
        {
            UIManager.Instance.StartRaceTimer();
        }
        else
        {
            Debug.LogWarning("�️ UIManager.Instance가 null�니");
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

        // Go!가 나온 후에 스쿠터 움직임 활성화
        if (player.TryGetComponent<scooterCtrl>(out var scooter))
            scooter.enabled = true;

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
        
        // �� 카메무빙 중단
        if (cameraController != null)
        {
            cameraController.StopGameCamera();
            Debug.Log("�� 카메무빙 중단!");
        }
        
        foreach (var bot in bots)
            bot.enabled = false;
            
        Debug.Log("�이종료!");
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
