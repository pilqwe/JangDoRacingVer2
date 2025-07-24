using UnityEngine;
using System.Collections;

public class ItemManager : MonoBehaviour
{
    public enum ItemType { Boost, Ink }
    [Header("아이템 타입 설정")]
    public ItemType itemType = ItemType.Boost;
    [Header("먹물 효과 지속 시간 (초)")]
    public float inkDuration = 3f;
    [Header("리스폰 시간 (초)")]
    public float respawnTime = 5f;

    // 아이템의 시각적 요소들
    private MeshRenderer[] meshRenderers;
    private Collider itemCollider;
    private bool isRespawning = false;

    private void Start()
    {
        // 시각적 요소들을 찾아서 저장
        meshRenderers = GetComponentsInChildren<MeshRenderer>();
        itemCollider = GetComponent<Collider>();
        
        if (itemCollider == null)
        {
            Debug.LogError($"[ItemManager] {gameObject.name}에 Collider가 없습니다!");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // 이미 리스폰 중이면 무시
        if (isRespawning) return;
        
        if (!other.CompareTag("Player")) return;

        Debug.Log($"[ItemManager] 플레이어가 {itemType} 아이템 획득!");

        switch (itemType)
        {
            case ItemType.Boost:
                // 부스트 게이지 100%로 설정
                var scooter = other.GetComponent<scooterCtrl>();
                if (scooter != null)
                {
                    scooter.SetBoostGaugeToMax();
                    Debug.Log("[ItemManager] 부스트 게이지가 최대로 충전되었습니다!");
                }
                
                // 🎵 부스트 아이템 사운드 재생
                if (SoundManager.Instance != null)
                {
                    SoundManager.Instance.PlayBoostItemSound();
                }
                break;
                
            case ItemType.Ink:
                // 먹물 효과 호출
                var uiManager = FindFirstObjectByType<UIManager>();
                if (uiManager != null)
                {
                    uiManager.ShowInkEffect(inkDuration);
                    Debug.Log("[ItemManager] 먹물 효과가 적용되었습니다!");
                }
                
                // 🎵 먹물 아이템 사운드 재생
                if (SoundManager.Instance != null)
                {
                    SoundManager.Instance.PlayInkItemSound();
                }
                break;
        }
        
        // 🎵 일반 아이템 획득 사운드 재생
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayItemPickupSound();
        }
        
        // 아이템 숨기기 및 리스폰 시작
        StartCoroutine(RespawnCoroutine());
    }

    private IEnumerator RespawnCoroutine()
    {
        isRespawning = true;
        
        // 아이템 숨기기 (MeshRenderer와 Collider 비활성화)
        HideItem();
        Debug.Log($"[ItemManager] {gameObject.name} 아이템 숨김 완료");
        
        // 리스폰 시간 대기
        yield return new WaitForSeconds(respawnTime);
        
        // 아이템 다시 보이기
        ShowItem();
        Debug.Log($"[ItemManager] {gameObject.name} 아이템 리스폰 완료! 위치: {transform.position}");
        
        isRespawning = false;
    }

    private void HideItem()
    {
        // 모든 MeshRenderer 비활성화
        foreach (var renderer in meshRenderers)
        {
            if (renderer != null)
                renderer.enabled = false;
        }
        
        // Collider 비활성화
        if (itemCollider != null)
            itemCollider.enabled = false;
    }

    private void ShowItem()
    {
        // 모든 MeshRenderer 활성화
        foreach (var renderer in meshRenderers)
        {
            if (renderer != null)
                renderer.enabled = true;
        }
        
        // Collider 활성화
        if (itemCollider != null)
            itemCollider.enabled = true;
    }

    void Update()
    {
        // Y축 기준으로 초당 90도 회전 (속도는 원하는 값으로 조절)
        transform.Rotate(Vector3.up * 90f * Time.deltaTime);
    }
}
