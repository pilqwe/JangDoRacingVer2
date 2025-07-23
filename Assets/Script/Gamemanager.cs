using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;
using UnityEngine.Splines;
using System.Collections.Generic;


public class RacingGameManager : MonoBehaviour
{
    [Header("ê²Œì„ œì‘ UI")]
    public GameObject startBackgroundPanel;  // †• ê²Œì„ œì‘ ë°°ê²½ ¨ë„
    public GameObject startPanel;            // œì‘ ë²„íŠ¼ UI ¨ë„
    public Button startButton;               // œì‘ ë²„íŠ¼
    
    [Header("ê²Œì„ ì§„í–‰ UI")]
    public GameObject restartPanel;          // ¬ì‹œë²„íŠ¼ UI ¨ë„
    public Button restartButton;             // ¬ì‹œë²„íŠ¼
    public GameObject pausePanel;            // ¼ì‹œ•ì ¨ë„ (ê³„ì†/¬ì‹œë²„íŠ¼ ¬í•¨)
    public Button resumeButton;              // ê³„ì† ë²„íŠ¼
    public Button pauseRestartButton;        // ¼ì‹œ•ì ¬ì‹œë²„íŠ¼
    public TMPro.TextMeshProUGUI countdownText; // ì¹´ìš´¸ë‹¤ìŠ¤(TextMeshPro)
    
    [Header("ê²Œì„ ¤ë¸ŒíŠ¸")]
    public GameObject player;                // Œë ˆ´ì–´ ¤ë¸ŒíŠ¸
    public SplineBotController[] bots;       // ¬ëŸ¬ ë´‡ì„ Inspectorì„œ °ê²°
    public int totalLaps = 3;                // ì´    public MeshRenderer[] startLights;       // Inspectorì„œ 3ê°œì˜ ¼ì´¤ë¸ŒíŠ¸ °ê²°
    public SplineAnimate splineAnimator;     // ¤í”Œ¼ì¸  ë‹ˆë©”ì´(Inspectorì„œ °ê²°)
    public CarCameraController cameraController; // †• ì¹´ë©”ì»¨íŠ¸ë¡¤ëŸ¬

    [Header("ë°°ê²½ „í™˜ ¨ê³¼")]
    public float backgroundFadeOutTime = 1.0f; // ë°°ê²½ ¬ë¼ì§€œê°„

    private int currentLap = 0;
    private bool raceStarted = false;
    private bool isPaused = false;

    private List<float> lapTimes = new List<float>();
    private float lapStartTime = 0f;
    private bool lapStarted = false; // €´ë¨¸ œì‘ ¬ë

    public static RacingGameManager Instance; // ±ê
    void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        // ESC ¤ë¡œ ¼ì‹œ•ì
        if (raceStarted && !isPaused && Input.GetKeyDown(KeyCode.Escape))
        {
            PauseGame();
        }
    }

    void Start()
    {
        // †• ê²Œì„ œì‘ ë°°ê²½ê³œì‘ ¨ë„ ëª¨ë‘ œì‹œ
        if (startBackgroundPanel != null)
            startBackgroundPanel.SetActive(true);
        
        startPanel.SetActive(true);
        countdownText.gameObject.SetActive(false);
        restartPanel.SetActive(false);
        pausePanel.SetActive(false);
        
        // ë²„íŠ¼ ´ë²¤°ê²°
        startButton.onClick.AddListener(OnStartButton);
        restartButton.onClick.AddListener(OnRestartButton);
        resumeButton.onClick.AddListener(OnResumeButton);
        pauseRestartButton.onClick.AddListener(OnRestartButton);
        
        // ë´‡ë“¤ ë¹„í™œ±í™”
        foreach (var bot in bots)
            bot.enabled = false;
            
        // †• ì¹´ë©”ì»¨íŠ¸ë¡¤ëŸ¬ ë™ ì°¾ê¸° (Inspectorì„œ  ë‹¹˜ì Šì ê²½ìš°)
        if (cameraController == null)
            cameraController = FindObjectOfType<CarCameraController>();
            
        // ì´ˆê¸°
        lapTimes.Clear();
        lapStartTime = Time.time;
        
        Debug.Log("® ê²Œì„ œì‘ ”ë©´ ì¤€ë¹„ë£Œ!");
    }

    public void OnStartButton()
    {
        Debug.Log(" œì‘ ë²„íŠ¼ ´ë¦­");
        
        if (startBackgroundPanel != null)
        {
            // ë°°ê²½ˆì„ ê²½ìš° ˜ì´„ì›ƒ ê²Œì„ œì‘
            StartCoroutine(StartGameSequence());
        }
        else
        {
            // ë°°ê²½†ì„ ê²½ìš° ë°”ë¡œ ê²Œì„ œì‘
            StartGameDirect();
        }
    }
    
    /// <summary>
    /// ¬ ë°°ê²½ ˜ì´„ì›ƒê³¨ê»˜ ê²Œì„ œì‘ œí€€    /// </summary>
    IEnumerator StartGameSequence()
    {
        Debug.Log("ŒŸ ê²Œì„ œì‘ œí€€œì‘!");
        
        // ë°°ê²½ ˜ì´„ì›ƒ
        yield return StartCoroutine(FadeOutBackground());
        
        // ê²Œì„ ì§ì ‘ œì‘
        StartGameDirect();
    }
    
    /// <summary>
    /// Œ˜ ë°°ê²½ ¨ë„ ˜ì´„ì›ƒ ì½”ë£¨    /// </summary>
    IEnumerator FadeOutBackground()
    {
        Debug.Log("Œ«ï¸ë°°ê²½ ˜ì´„ì›ƒ œì‘!");
        
        CanvasGroup backgroundCanvasGroup = startBackgroundPanel.GetComponent<CanvasGroup>();
        
        // CanvasGroup†ìœ¼ë©ì¶”ê
        if (backgroundCanvasGroup == null)
        {
            backgroundCanvasGroup = startBackgroundPanel.AddComponent<CanvasGroup>();
            Debug.Log("“ CanvasGroup ì»´í¬ŒíŠ¸ ë™ ì¶”ê");
        }
        
        float elapsedTime = 0f;
        float startAlpha = backgroundCanvasGroup.alpha;
        
        while (elapsedTime < backgroundFadeOutTime)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / backgroundFadeOutTime;
            
            // ë¶€œëŸ¬˜ì´„ì›ƒ
            backgroundCanvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, progress);
            
            yield return null;
        }
        
        // „ì „¬ëª…˜ê²Œ ˜ê³  ë¹„í™œ±í™”
        backgroundCanvasGroup.alpha = 0f;
        startBackgroundPanel.SetActive(false);
        
        Debug.Log("ë°°ê²½ ˜ì´„ì›ƒ „ë£Œ!");
    }
    
    /// <summary>
    ///  ê²Œì„ ì§ì ‘ œì‘ (ê¸°ì¡´ ë¡œì§)
    /// </summary>
    void StartGameDirect()
    {
        Debug.Log("¯ ê²Œì„ ì§ì ‘ œì‘!");
        
        startPanel.SetActive(false);
        
        // †• ì¹´ë©”ë¬´ë¹™ì¹´ìš´¸ë‹¤„ì— ë¨¼ì œì‘
        if (cameraController != null)
        {
            // †• ì»¤ìŠ¤€ ì¹´ë©”ë¬´ë¹™ˆëŠ” ê²½ìš° „ë£Œ ì¹´ìš´¸ë‹¤œì‘
            if (cameraController.useCustomCameraMoving && cameraController.cameraWaypoints != null && cameraController.cameraWaypoints.Length > 0)
            {
                // ì¹´ë©”ë¬´ë¹™ „ë£Œ ´ë²¤¸ì— ì¹´ìš´¸ë‹¤œì‘ °ê²°
                cameraController.OnCustomCameraMovingComplete = OnCustomCameraMovingComplete;
                cameraController.StartGameCamera();
                Debug.Log("“¹ ì»¤ìŠ¤€ ì¹´ë©”ë¬´ë¹™ œì‘! „ë£Œ ì¹´ìš´¸ë‹¤ˆì •");
            }
            else
            {
                // ê¸°ë³¸ ì¹´ë©”¼ì¸ ê²½ìš° ë°”ë¡œ ì¹´ìš´¸ë‹¤œì‘
                cameraController.StartGameCamera();
                StartCoroutine(StartCountdown());
                Debug.Log("“¹ ê¸°ë³¸ ì¹´ë©”ë¬´ë¹™ œì‘ + ì¹´ìš´¸ë‹¤œì‘!");
            }
        }
        else
        {
            Debug.LogWarning(" ï¸ ì¹´ë©”ì»¨íŠ¸ë¡¤ëŸ¬ê°€ null…ë‹ˆ");
            StartCoroutine(StartCountdown());
        }
    }
    
    /// <summary>
    /// ¬ ì»¤ìŠ¤€ ì¹´ë©”ë¬´ë¹™ „ë£Œ ¸ì¶œ˜ëŠ” ì½œë°±
    /// </summary>
    void OnCustomCameraMovingComplete()
    {
        Debug.Log("¬ ì»¤ìŠ¤€ ì¹´ë©”ë¬´ë¹™ „ë£Œ! ´ì œ ì¹´ìš´¸ë‹¤œì‘");
        StartCoroutine(StartCountdown());
    }

    IEnumerator StartCountdown()
    {
        countdownText.gameObject.SetActive(true);
        // ëª¨ë“  ¼ì´êº¼ë‘ê¸        foreach (var light in startLights)
            light.enabled = false;

        string[] counts = { "3", "2", "1", "Go!" };
        for (int i = 0; i < counts.Length; i++)
        {
            countdownText.text = counts[i];
            Debug.Log($"ì¹´ìš´¸ë‹¤ {counts[i]}");

            // 3,2,1ë§ì¶° ¼ì´ì¼œê¸°
            if (i < startLights.Length)
                startLights[i].enabled = true;

            yield return new WaitForSeconds(1f);
        }
        countdownText.gameObject.SetActive(false);
        Debug.Log("ì¹´ìš´¸ë‹¤ìŠ¤ë¹„í™œ±í™”);

        // ëª¨ë“  ¼ì´„ê¸° (¹ì Go!ë§ì¶° °ì¶œ)
        foreach (var light in startLights)
            light.enabled = false;

        StartRace();
    }

    void StartRace()
    {
        // UIManager ì¡´ì¬ •ì¸
        if (UIManager.Instance != null)
        {
            UIManager.Instance.StartRaceTimer();
        }
        else
        {
            Debug.LogWarning(" ï¸ UIManager.Instanceê°€ null…ë‹ˆ");
        }
        
        // ¤í”Œ¼ì¸  ë‹ˆë©”ì´œì‘
        if (splineAnimator != null)
        {
            splineAnimator.Play(); 
            Debug.Log("¯ ¤í”Œ¼ì¸  ë‹ˆë©”ì´œì‘!");
        }
        else
        {
            Debug.LogWarning(" ï¸ splineAnimatorê°€ null…ë‹ˆ");
        }
        
        raceStarted = true;
        currentLap = 1;

        // Go!ê°€ ˜ì˜¨ ¤ì— ¤ì¿ €ì§ì„ œì„±        if (player.TryGetComponent<scooterCtrl>(out var scooter))
            scooter.enabled = true;

        foreach (var bot in bots)
            bot.enabled = true;
            
        Debug.Log(" ˆì´œì‘ „ë£Œ!");
    }

    // ¸ë¦¬ê±°ì—¸ì¶œ
    public void OnLapTrigger()
    {
        if (!lapStarted)
        {
            // ì²¸ë¦¬ê±µê³¼ €´ë¨¸ œì‘ (Lap 1)
            lapStarted = true;
            lapStartTime = Time.time;
            currentLap = 1;
            UIManager.Instance.UpdateLapUI(currentLap, totalLaps);
            return;
        }

        // ´í›„ë¶€€ê¸°ë¡
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
        
        // †• ì¹´ë©”ë¬´ë¹™ ì¤‘ë‹¨
        if (cameraController != null)
        {
            cameraController.StopGameCamera();
            Debug.Log("“¹ ì¹´ë©”ë¬´ë¹™ ì¤‘ë‹¨!");
        }
        
        foreach (var bot in bots)
            bot.enabled = false;
            
        Debug.Log("ˆì´ì¢…ë£Œ!");
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
