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
        // �� 게임 �작 배경곜작 �널 모두 �시
        if (startBackgroundPanel != null)
            startBackgroundPanel.SetActive(true);
        
        startPanel.SetActive(true);
        countdownText.gameObject.SetActive(false);
        restartPanel.SetActive(false);
        pausePanel.SetActive(false);
        
        // 버튼 �벤�결
        startButton.onClick.AddListener(OnStartButton);
        restartButton.onClick.AddListener(OnRestartButton);
        resumeButton.onClick.AddListener(OnResumeButton);
        pauseRestartButton.onClick.AddListener(OnRestartButton);
        
        // 봇들 비활�화
        foreach (var bot in bots)
            bot.enabled = false;
            
        // �� 카메컨트롤러 �동 찾기 (Inspector�서 �당�� �� 경우)
        if (cameraController == null)
            cameraController = FindObjectOfType<CarCameraController>();
            
        // 초기
        lapTimes.Clear();
        lapStartTime = Time.time;
        
        Debug.Log("�� 게임 �작 �면 준비료!");
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
    /// �� 게임 직접 �작 (기존 로직)
    /// </summary>
    void StartGameDirect()
    {
        Debug.Log("�� 게임 직접 �작!");
        
        startPanel.SetActive(false);
        
        // �� 카메무빙카운�다�에 먼� �작
        if (cameraController != null)
        {
            // �� 커스� 카메무빙�는 경우 �료 카운�다�작
            if (cameraController.useCustomCameraMoving && cameraController.cameraWaypoints != null && cameraController.cameraWaypoints.Length > 0)
            {
                // 카메무빙 �료 �벤�에 카운�다�작 �결
                cameraController.OnCustomCameraMovingComplete = OnCustomCameraMovingComplete;
                cameraController.StartGameCamera();
                Debug.Log("�� 커스� 카메무빙 �작! �료 카운�다�정");
            }
            else
            {
                // 기본 카메�인 경우 바로 카운�다�작
                cameraController.StartGameCamera();
                StartCoroutine(StartCountdown());
                Debug.Log("�� 기본 카메무빙 �작 + 카운�다�작!");
            }
        }
        else
        {
            Debug.LogWarning("�️ 카메컨트롤러가 null�니");
            StartCoroutine(StartCountdown());
        }
    }
    
    /// <summary>
    /// �� 커스� 카메무빙 �료 �출�는 콜백
    /// </summary>
    void OnCustomCameraMovingComplete()
    {
        Debug.Log("�� 커스� 카메무빙 �료! �제 카운�다�작");
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
