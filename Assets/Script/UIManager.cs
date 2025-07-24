using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections; // ì¶”ê¤ì„¤í˜´ìŠ¤


public class UIManager : MonoBehaviour
{
    public static UIManager Instance;
    public scooterCtrl scooter;
    public TextMeshProUGUI speedText;
    public Slider boostGaugeSlider;
    public TextMeshProUGUI lapText;
    public TextMeshProUGUI lapTimeText; // ©ë³„ ê¸°ë¡ œì‹œ    public TextMeshProUGUI totalTimeText; // ¤ì‹œê°„ì²´ œê°„ œì‹œ(Inspectorì„œ °ê²°)
    public GameObject inkPanel; // ë¨¹ë¬¼ UI ¤ë¸ŒíŠ¸

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

        // ¤ì‹œê°„ì²´ œê°„ œì‹œ
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

    // F1 ¤í€ê¸°ë¡ œì‹œ
    public void UpdateLapTimeList(List<float> lapTimes)
    {
        if (lapTimeText == null) return;

        if (lapTimes.Count == 0)
        {
            lapTimeText.text = "Lap ê¸°ë¡ †ìŒ";
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

    public void ShowInkEffect(float duration)
    {
        if (inkPanel != null)
            StartCoroutine(InkEffectFadeRoutine(duration));
    }

    private IEnumerator InkEffectFadeRoutine(float duration)
    {
        // CanvasGroup ì»´í¬ŒíŠ¸ê°€ †ìœ¼ë©ë™ ì¶”ê
        CanvasGroup cg = inkPanel.GetComponent<CanvasGroup>();
        if (cg == null)
            cg = inkPanel.AddComponent<CanvasGroup>();

        inkPanel.SetActive(true);

        // ˜ì´(0 1)
        float fadeInTime = 0.5f;
        float t = 0f;
        while (t < fadeInTime)
        {
            t += Time.deltaTime;
            cg.alpha = Mathf.Lerp(0f, 1f, t / fadeInTime);
            yield return null;
        }
        cg.alpha = 1f;

        // ë¨¹ë¬¼  ì
        yield return new WaitForSeconds(duration);

        // ˜ì´„ì›ƒ (1 0)
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
