using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections; // 추가된 네임스페이스


public class UIManager : MonoBehaviour
{
    public static UIManager Instance;
    public scooterCtrl scooter;
    public TextMeshProUGUI speedText;
    public Slider boostGaugeSlider;
    public TextMeshProUGUI lapText;
    public TextMeshProUGUI lapTimeText; // 랩별 기록 표시용
    public TextMeshProUGUI totalTimeText; // 실시간 전체 시간 표시용 (Inspector에서 연결)
    public GameObject inkPanel; // 먹물 UI 오브젝트

    private float raceStartTime = 0f;
    private bool raceStarted = false;

    void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        if (scooter != null && speedText != null)
            speedText.text = $"{scooter.CurrentSpeed:F1} km/h";

        if (scooter != null && boostGaugeSlider != null)
            boostGaugeSlider.value = scooter.DriftGauge / scooter.MaxDriftGauge;

        // 실시간 전체 시간 표시
        if (raceStarted && totalTimeText != null)
        {
            float elapsed = Time.time - raceStartTime;
            totalTimeText.text = $"Total Time: {elapsed:00.000} sec";
        }


    }

    public void StartRaceTimer()
    {
        raceStartTime = Time.time;
        raceStarted = true;
    }

    public void UpdateLapUI(int currentLap, int totalLap)
    {
        if (lapText != null)
            lapText.text = $"LAP {currentLap} / {totalLap}";
    }

    // F1 스타일 랩 타임 기록 표시
    public void UpdateLapTimeList(List<float> lapTimes)
    {
        if (lapTimeText == null) return;

        if (lapTimes.Count == 0)
        {
            lapTimeText.text = "Lap 기록 없음";
            return;
        }

        string result = "";
        float bestTime = float.MaxValue;
        int bestLap = -1;

        for (int i = 0; i < lapTimes.Count; i++)
        {
            string timeStr = $"{lapTimes[i]:00.000} sec";
            if (lapTimes[i] < bestTime)
            {
                bestTime = lapTimes[i];
                bestLap = i;
            }
            result += $"Lap {i + 1}: {timeStr}\n";
        }

        result += $"\nBest Lap {bestLap + 1} - {bestTime:00.000} sec";
        lapTimeText.text = result;
    }

    public void ShowInkEffect(float duration)
    {
        if (inkPanel != null)
            StartCoroutine(InkEffectFadeRoutine(duration));
    }

    private IEnumerator InkEffectFadeRoutine(float duration)
    {
        // CanvasGroup 컴포넌트가 없으면 자동 추가
        CanvasGroup cg = inkPanel.GetComponent<CanvasGroup>();
        if (cg == null)
            cg = inkPanel.AddComponent<CanvasGroup>();

        inkPanel.SetActive(true);

        // 페이드 인 (0 → 1)
        float fadeInTime = 0.5f;
        float t = 0f;
        while (t < fadeInTime)
        {
            t += Time.deltaTime;
            cg.alpha = Mathf.Lerp(0f, 1f, t / fadeInTime);
            yield return null;
        }
        cg.alpha = 1f;

        // 먹물 유지
        yield return new WaitForSeconds(duration);

        // 페이드 아웃 (1 → 0)
        float fadeOutTime = 0.5f;
        t = 0f;
        while (t < fadeOutTime)
        {
            t += Time.deltaTime;
            cg.alpha = Mathf.Lerp(1f, 0f, t / fadeOutTime);
            yield return null;
        }
        cg.alpha = 0f;
        inkPanel.SetActive(false);
    }
}
