using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;
using UnityEngine.Splines;
using System.Collections.Generic;


public class RacingGameManager : MonoBehaviour
{

    public GameObject startPanel;      // 시작 버튼 UI 패널
    public Button startButton;         // 시작 버튼
    public GameObject restartPanel;    // 재시작 버튼 UI 패널
    public Button restartButton;       // 재시작 버튼
    public GameObject pausePanel;      // 일시정지 패널 (계속/재시작 버튼 포함)
    public Button resumeButton;        // 계속 버튼
    public Button pauseRestartButton;  // 일시정지 내 재시작 버튼
    public TMPro.TextMeshProUGUI countdownText; // 카운트다운 텍스트 (TextMeshPro)
    public GameObject player;          // 플레이어 오브젝트
    public SplineBotController[] bots; // 여러 봇을 Inspector에서 연결
    public int totalLaps = 3;          // 총 랩 수
    public MeshRenderer[] startLights; // Inspector에서 3개의 라이트 오브젝트 연결

    public SplineAnimate splineAnimator; // 스플라인 애니메이터 (Inspector에서 연결)

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
        startPanel.SetActive(true);
        countdownText.gameObject.SetActive(false);
        restartPanel.SetActive(false);
        pausePanel.SetActive(false);
        startButton.onClick.AddListener(OnStartButton);
        restartButton.onClick.AddListener(OnRestartButton);
        resumeButton.onClick.AddListener(OnResumeButton);
        pauseRestartButton.onClick.AddListener(OnRestartButton);
        foreach (var bot in bots)
            bot.enabled = false;
        // 플레이어는 항상 활성화
        // ESC 키 일시정지는 Update에서 처리
        lapTimes.Clear();
        lapStartTime = Time.time;
    }

    void OnStartButton()
    {
        startPanel.SetActive(false);
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
        UIManager.Instance.StartRaceTimer();
        splineAnimator.Play(); // 스플라인 애니메이션 시작
        raceStarted = true;
        currentLap = 1;
        player.SetActive(true);
        foreach (var bot in bots)
            bot.enabled = true;
        // 필요시 플레이어/봇 위치 초기화
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
