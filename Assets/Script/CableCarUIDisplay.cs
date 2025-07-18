using UnityEngine;

public class CableCarUIDisplay : MonoBehaviour
{
    private CableCarSystem cableCarSystem;
    private bool showDebugUI = true;

    void Start()
    {
        cableCarSystem = FindObjectOfType<CableCarSystem>();
    }

    void Update()
    {
        // F2 키로 UI 표시/숨김 토글
        if (Input.GetKeyDown(KeyCode.F2))
        {
            showDebugUI = !showDebugUI;
        }

        // F3 키로 수동 케이블카 스폰
        if (Input.GetKeyDown(KeyCode.F3) && cableCarSystem != null)
        {
            cableCarSystem.ManualSpawn();
        }

        // F4 키로 모든 케이블카 제거
        if (Input.GetKeyDown(KeyCode.F4) && cableCarSystem != null)
        {
            cableCarSystem.ManualDespawnAll();
        }
    }

    void OnGUI()
    {
        if (!showDebugUI || cableCarSystem == null) return;

        // 스타일 설정
        GUIStyle boxStyle = new GUIStyle(GUI.skin.box);
        boxStyle.fontSize = 14;
        boxStyle.normal.textColor = Color.white;

        GUIStyle labelStyle = new GUIStyle(GUI.skin.label);
        labelStyle.fontSize = 12;
        labelStyle.normal.textColor = Color.white;

        GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
        buttonStyle.fontSize = 11;

        // 케이블카 시스템 정보 박스
        GUI.Box(new Rect(Screen.width - 280, 10, 270, 160), "🚡 케이블카 시스템", boxStyle);

        // 활성 케이블카 수
        int activeCars = 0;
        CableCarController[] controllers = FindObjectsOfType<CableCarController>();
        activeCars = controllers.Length;

        GUI.Label(new Rect(Screen.width - 270, 35, 250, 20),
            $"활성 케이블카: {activeCars} / {cableCarSystem.maxCableCars}", labelStyle);

        // 자동 스폰 상태
        GUI.Label(new Rect(Screen.width - 270, 55, 250, 20),
            $"자동 스폰: {(cableCarSystem.autoSpawn ? "ON" : "OFF")}", labelStyle);

        // 스폰 간격
        GUI.Label(new Rect(Screen.width - 270, 75, 250, 20),
            $"스폰 간격: {cableCarSystem.spawnInterval:F1}초", labelStyle);

        // 개별 케이블카 정보
        if (controllers.Length > 0)
        {
            GUI.Label(new Rect(Screen.width - 270, 95, 250, 20),
                "케이블카 상태:", labelStyle);

            for (int i = 0; i < Mathf.Min(controllers.Length, 3); i++)
            {
                if (controllers[i] != null)
                {
                    float progress = controllers[i].Progress * 100f;
                    GUI.Label(new Rect(Screen.width - 270, 115 + i * 15, 250, 15),
                        $"  #{i + 1}: {progress:F0}% 완료", labelStyle);
                }
            }
        }

        // 컨트롤 버튼들
        if (GUI.Button(new Rect(Screen.width - 270, 180, 80, 25), "스폰", buttonStyle))
        {
            cableCarSystem.ManualSpawn();
        }

        if (GUI.Button(new Rect(Screen.width - 185, 180, 80, 25), "모두 제거", buttonStyle))
        {
            cableCarSystem.ManualDespawnAll();
        }

        if (GUI.Button(new Rect(Screen.width - 100, 180, 80, 25),
            cableCarSystem.autoSpawn ? "자동 OFF" : "자동 ON", buttonStyle))
        {
            if (cableCarSystem.autoSpawn)
            {
                cableCarSystem.StopAutoSpawn();
                cableCarSystem.autoSpawn = false;
            }
            else
            {
                cableCarSystem.autoSpawn = true;
                cableCarSystem.StartAutoSpawn();
            }
        }

        // 조작법 안내
        GUI.Box(new Rect(Screen.width - 280, 215, 270, 80), "🎮 케이블카 조작법", boxStyle);
        GUI.Label(new Rect(Screen.width - 270, 240, 250, 20), "F2: UI 표시/숨김", labelStyle);
        GUI.Label(new Rect(Screen.width - 270, 255, 250, 20), "F3: 수동 스폰", labelStyle);
        GUI.Label(new Rect(Screen.width - 270, 270, 250, 20), "F4: 모두 제거", labelStyle);
    }
}
