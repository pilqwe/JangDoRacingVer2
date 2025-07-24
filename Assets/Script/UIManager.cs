using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;


public class UIManager : MonoBehaviour
{
    public static UIManager Instance;
    public scooterCtrl scooter;
    public TextMeshProUGUI speedText;
    public Slider boostGaugeSlider;
    public TextMeshProUGUI lapText;
    public TextMeshProUGUI lapTimeText; // 랩별 기록 표시용
    public TextMeshProUGUI totalTimeText; // 실시간 전체 시간 표시용 (Inspector에서 연결)

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
            Debug.Log($"[UIManager] totalTimeText 업데이트: {totalTimeText.text}, raceStarted={raceStarted}, raceStartTime={raceStartTime}");
        }
        else
        {
            Debug.Log($"[UIManager] totalTimeText 조건 미충족: raceStarted={raceStarted}, totalTimeText null? {totalTimeText == null}");
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
            result += $"LAP {i + 1}: {timeStr}\n";
        }

        //result += $"\nBest Lap {bestLap + 1} - {bestTime:00.000} sec";
        lapTimeText.text = result;
    }
}
