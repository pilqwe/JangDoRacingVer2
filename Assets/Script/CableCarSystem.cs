using UnityEngine;
using System.Collections;

public class CableCarSystem : MonoBehaviour
{
    [Header("케이블카 설정")]
    public GameObject cableCarPrefab;           // 케이블카 프리팹
    public Transform[] waypoints;               // 케이블카가 이동할 경로
    public float moveSpeed = 5f;                // 이동 속도
    public float spawnInterval = 10f;           // 스폰 간격 (초)
    public int maxCableCars = 3;                // 최대 케이블카 수

    [Header("스폰 설정")]
    public bool autoSpawn = true;               // 자동 스폰 여부
    public float despawnDistance = 50f;         // 플레이어로부터 일정 거리에서 제거
    public Transform playerTransform;           // 플레이어 Transform (거리 계산용)

    [Header("애니메이션 설정")]
    public bool swayAnimation = true;           // 흔들림 애니메이션
    public float swayAmount = 0.5f;             // 흔들림 정도
    public float swaySpeed = 1f;                // 흔들림 속도

    [Header("오디오 설정")]
    public AudioClip cableCarSound;             // 케이블카 소리
    public float audioVolume = 0.3f;            // 오디오 볼륨

    private System.Collections.Generic.List<GameObject> activeCableCars;
    private Coroutine spawnCoroutine;

    void Start()
    {
        activeCableCars = new System.Collections.Generic.List<GameObject>();

        // 플레이어를 찾지 못했다면 scooterCtrl을 가진 객체를 찾기
        if (playerTransform == null)
        {
            scooterCtrl scooter = FindObjectOfType<scooterCtrl>();
            if (scooter != null)
                playerTransform = scooter.transform;
        }

        // 웨이포인트가 설정되지 않았다면 기본 웨이포인트 생성
        if (waypoints == null || waypoints.Length == 0)
        {
            CreateDefaultWaypoints();
        }

        // 자동 스폰 시작
        if (autoSpawn)
        {
            StartAutoSpawn();
        }
    }

    void Update()
    {
        // 거리에 따른 케이블카 제거 체크
        if (playerTransform != null)
        {
            CheckDistanceAndDespawn();
        }
    }

    /// <summary>
    /// 자동 스폰 시작
    /// </summary>
    public void StartAutoSpawn()
    {
        if (spawnCoroutine == null)
        {
            spawnCoroutine = StartCoroutine(AutoSpawnCoroutine());
        }
    }

    /// <summary>
    /// 자동 스폰 중지
    /// </summary>
    public void StopAutoSpawn()
    {
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
            spawnCoroutine = null;
        }
    }

    /// <summary>
    /// 자동 스폰 코루틴
    /// </summary>
    private IEnumerator AutoSpawnCoroutine()
    {
        while (autoSpawn)
        {
            if (activeCableCars.Count < maxCableCars && cableCarPrefab != null && waypoints.Length > 0)
            {
                SpawnCableCar();
            }

            yield return new WaitForSeconds(spawnInterval);
        }
    }

    /// <summary>
    /// 케이블카 스폰
    /// </summary>
    public void SpawnCableCar()
    {
        if (cableCarPrefab == null || waypoints.Length == 0)
        {
            Debug.LogWarning("케이블카 프리팹 또는 웨이포인트가 설정되지 않았습니다!");
            return;
        }

        // 첫 번째 웨이포인트에서 스폰
        Vector3 spawnPosition = waypoints[0].position;
        Quaternion spawnRotation = waypoints.Length > 1 ?
            Quaternion.LookRotation(waypoints[1].position - waypoints[0].position) :
            Quaternion.identity;

        GameObject newCableCar = Instantiate(cableCarPrefab, spawnPosition, spawnRotation);

        // 케이블카 컨트롤러 추가
        CableCarController controller = newCableCar.GetComponent<CableCarController>();
        if (controller == null)
        {
            controller = newCableCar.AddComponent<CableCarController>();
        }

        // 컨트롤러 설정
        controller.Initialize(waypoints, moveSpeed, swayAnimation, swayAmount, swaySpeed, this);

        // 오디오 소스 추가 및 설정
        if (cableCarSound != null)
        {
            AudioSource audioSource = newCableCar.GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = newCableCar.AddComponent<AudioSource>();
            }

            audioSource.clip = cableCarSound;
            audioSource.volume = audioVolume;
            audioSource.loop = true;
            audioSource.spatialBlend = 1f; // 3D 사운드
            audioSource.Play();
        }

        activeCableCars.Add(newCableCar);

        Debug.Log($"🚡 케이블카 스폰! 현재 활성 케이블카: {activeCableCars.Count}");
    }

    /// <summary>
    /// 거리에 따른 케이블카 제거
    /// </summary>
    private void CheckDistanceAndDespawn()
    {
        for (int i = activeCableCars.Count - 1; i >= 0; i--)
        {
            if (activeCableCars[i] == null)
            {
                activeCableCars.RemoveAt(i);
                continue;
            }

            float distance = Vector3.Distance(playerTransform.position, activeCableCars[i].transform.position);

            if (distance > despawnDistance)
            {
                DespawnCableCar(activeCableCars[i]);
                activeCableCars.RemoveAt(i);
            }
        }
    }

    /// <summary>
    /// 케이블카 제거
    /// </summary>
    public void DespawnCableCar(GameObject cableCar)
    {
        if (cableCar != null)
        {
            Debug.Log("🗑️ 케이블카 제거!");
            Destroy(cableCar);
        }
    }

    /// <summary>
    /// 모든 케이블카 제거
    /// </summary>
    public void DespawnAllCableCars()
    {
        foreach (GameObject cableCar in activeCableCars)
        {
            if (cableCar != null)
            {
                Destroy(cableCar);
            }
        }
        activeCableCars.Clear();
        Debug.Log("🧹 모든 케이블카 제거 완료!");
    }

    /// <summary>
    /// 기본 웨이포인트 생성 (테스트용)
    /// </summary>
    private void CreateDefaultWaypoints()
    {
        waypoints = new Transform[4];

        for (int i = 0; i < 4; i++)
        {
            GameObject waypoint = new GameObject($"CableCar_Waypoint_{i}");
            waypoint.transform.SetParent(transform);
            // 더 현실적인 산악 케이블카 경로 예시
            waypoint.transform.position = transform.position + new Vector3(
                i * 15f,           // X: 15미터씩 이동
                10f + i * 5f,      // Y: 점점 높아짐 (5미터씩)
                i * 3f             // Z: 약간 구불구불하게
            );
            waypoints[i] = waypoint.transform; // 이 줄이 누락되어 있었습니다!
        }

        Debug.Log("기본 웨이포인트가 생성되었습니다. Inspector에서 수정하세요!");
    }

    /// <summary>
    /// 수동으로 케이블카 스폰 (테스트용)
    /// </summary>
    [ContextMenu("케이블카 스폰")]
    public void ManualSpawn()
    {
        SpawnCableCar();
    }

    /// <summary>
    /// 수동으로 모든 케이블카 제거 (테스트용)
    /// </summary>
    [ContextMenu("모든 케이블카 제거")]
    public void ManualDespawnAll()
    {
        DespawnAllCableCars();
    }

    // 기즈모로 웨이포인트 경로 표시
    void OnDrawGizmos()
    {
        if (waypoints == null || waypoints.Length < 2) return;

        Gizmos.color = Color.yellow;

        for (int i = 0; i < waypoints.Length; i++)
        {
            if (waypoints[i] == null) continue;

            // 웨이포인트 표시
            Gizmos.DrawWireSphere(waypoints[i].position, 1f);

            // 다음 웨이포인트로의 선 그리기
            if (i < waypoints.Length - 1 && waypoints[i + 1] != null)
            {
                Gizmos.DrawLine(waypoints[i].position, waypoints[i + 1].position);
            }
        }

        // 플레이어 주변 제거 범위 표시
        if (playerTransform != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(playerTransform.position, despawnDistance);
        }
    }

    void OnDestroy()
    {
        StopAutoSpawn();
        DespawnAllCableCars();
    }
}
