using UnityEngine;
using System.Collections;

public class CableCarEffects : MonoBehaviour
{
    [Header("파티클 이펙트")]
    public ParticleSystem[] smokeEffects;      // 연기 이펙트
    public ParticleSystem[] sparkEffects;     // 스파크 이펙트 (케이블 마찰)

    [Header("라이트 이펙트")]
    public Light[] cableCarLights;            // 케이블카 조명
    public bool flickerLights = true;         // 조명 깜빡임
    public float flickerSpeed = 2f;           // 깜빡임 속도

    [Header("사운드 이펙트")]
    public AudioSource movementAudio;         // 이동 소리
    public AudioClip[] randomSounds;          // 랜덤 소리들 (삐걱거림 등)
    public float soundPlayChance = 0.1f;      // 소리 재생 확률

    [Header("그림자 이펙트")]
    public GameObject shadowPrefab;           // 그림자 프리팹
    public LayerMask groundLayer = 1;         // 지면 레이어

    private CableCarController controller;
    private GameObject currentShadow;
    private float lightFlickerTimer = 0f;
    private float soundTimer = 0f;

    void Start()
    {
        controller = GetComponent<CableCarController>();

        // 그림자 생성
        if (shadowPrefab != null)
        {
            CreateShadow();
        }

        // 파티클 시스템 시작
        StartParticleEffects();

        // 조명 초기화
        InitializeLights();
    }

    void Update()
    {
        UpdateShadow();
        UpdateLightFlicker();
        UpdateSoundEffects();
        UpdateParticleIntensity();
    }

    private void StartParticleEffects()
    {
        // 연기 이펙트 시작
        if (smokeEffects != null)
        {
            foreach (var smoke in smokeEffects)
            {
                if (smoke != null)
                {
                    smoke.Play();
                }
            }
        }

        // 스파크 이펙트는 간헐적으로 재생
        if (sparkEffects != null)
        {
            StartCoroutine(RandomSparkEffects());
        }
    }

    private void UpdateParticleIntensity()
    {
        if (controller == null) return;

        // 이동 속도에 따라 파티클 강도 조절
        bool isMoving = controller.IsMoving;

        if (smokeEffects != null)
        {
            foreach (var smoke in smokeEffects)
            {
                if (smoke != null)
                {
                    var emission = smoke.emission;
                    emission.rateOverTime = isMoving ? 10f : 2f;
                }
            }
        }
    }

    private IEnumerator RandomSparkEffects()
    {
        while (gameObject != null)
        {
            yield return new WaitForSeconds(Random.Range(2f, 8f));

            if (sparkEffects != null && sparkEffects.Length > 0)
            {
                int randomIndex = Random.Range(0, sparkEffects.Length);
                if (sparkEffects[randomIndex] != null)
                {
                    sparkEffects[randomIndex].Play();
                }
            }
        }
    }

    private void InitializeLights()
    {
        if (cableCarLights != null)
        {
            foreach (var light in cableCarLights)
            {
                if (light != null)
                {
                    light.enabled = true;
                    light.intensity = Random.Range(0.8f, 1.2f);
                }
            }
        }
    }

    private void UpdateLightFlicker()
    {
        if (!flickerLights || cableCarLights == null) return;

        lightFlickerTimer += Time.deltaTime * flickerSpeed;

        foreach (var light in cableCarLights)
        {
            if (light != null)
            {
                // 부드러운 깜빡임 효과
                float flicker = Mathf.PerlinNoise(lightFlickerTimer + light.GetInstanceID(), 0f);
                light.intensity = Mathf.Lerp(0.5f, 1.5f, flicker);
            }
        }
    }

    private void UpdateSoundEffects()
    {
        if (randomSounds == null || randomSounds.Length == 0) return;

        soundTimer += Time.deltaTime;

        if (soundTimer >= 1f) // 1초마다 체크
        {
            soundTimer = 0f;

            if (Random.value < soundPlayChance && controller != null && controller.IsMoving)
            {
                PlayRandomSound();
            }
        }
    }

    private void PlayRandomSound()
    {
        if (randomSounds.Length > 0)
        {
            AudioClip randomClip = randomSounds[Random.Range(0, randomSounds.Length)];
            if (randomClip != null)
            {
                AudioSource.PlayClipAtPoint(randomClip, transform.position, 0.3f);
            }
        }
    }

    private void CreateShadow()
    {
        currentShadow = Instantiate(shadowPrefab);
        currentShadow.name = "CableCar_Shadow";

        // 그림자를 케이블카의 자식으로 설정하지 않음 (독립적으로 움직임)
    }

    private void UpdateShadow()
    {
        if (currentShadow == null) return;

        // 레이캐스팅으로 지면 찾기
        RaycastHit hit;
        Vector3 rayStart = transform.position + Vector3.up * 50f;

        if (Physics.Raycast(rayStart, Vector3.down, out hit, 100f, groundLayer))
        {
            // 그림자를 지면에 투영
            currentShadow.transform.position = hit.point + Vector3.up * 0.1f;
            currentShadow.SetActive(true);

            // 거리에 따른 그림자 크기 조절
            float distance = Vector3.Distance(transform.position, hit.point);
            float scale = Mathf.Clamp(1f - (distance - 10f) / 20f, 0.3f, 1f);
            currentShadow.transform.localScale = Vector3.one * scale;

            // 그림자 투명도 조절
            Renderer shadowRenderer = currentShadow.GetComponent<Renderer>();
            if (shadowRenderer != null)
            {
                Color shadowColor = shadowRenderer.material.color;
                shadowColor.a = scale * 0.5f;
                shadowRenderer.material.color = shadowColor;
            }
        }
        else
        {
            currentShadow.SetActive(false);
        }
    }

    // 케이블카가 웨이포인트에 도달했을 때 호출될 수 있는 이벤트
    public void OnWaypointReached(int waypointIndex)
    {
        // 특정 웨이포인트에서 특별한 이펙트 재생
        if (waypointIndex == 0) // 시작점
        {
            PlayWaypointEffect("start");
        }
        else if (controller != null && waypointIndex >= controller.CurrentWaypointIndex - 1) // 종점
        {
            PlayWaypointEffect("end");
        }
    }

    private void PlayWaypointEffect(string effectType)
    {
        switch (effectType)
        {
            case "start":
                // 시작 이펙트 (예: 경적 소리)
                Debug.Log("🚡 케이블카 출발!");
                break;

            case "end":
                // 종료 이펙트 (예: 브레이크 소리)
                Debug.Log("🛑 케이블카 도착!");
                break;
        }
    }

    void OnDestroy()
    {
        // 그림자 정리
        if (currentShadow != null)
        {
            Destroy(currentShadow);
        }
    }

    // 기즈모로 이펙트 범위 표시
    void OnDrawGizmosSelected()
    {
        // 그림자 투영 범위 표시
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(transform.position, Vector3.down * 50f);

        // 사운드 범위 표시
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 10f);
    }
}
