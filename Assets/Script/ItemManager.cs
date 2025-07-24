using UnityEngine;
using System.Collections; // ← 추가

public class ItemManager : MonoBehaviour
{
    public enum ItemType { Boost, Ink }
    [Header("아이템 타입 설정")]
    public ItemType itemType = ItemType.Boost;
    [Header("먹물 효과 지속 시간 (초)")]
    public float inkDuration = 3f;

    public GameObject itemPrefab; // Inspector에서 프리팹 연결

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        switch (itemType)
        {
            case ItemType.Boost:
                // 부스트 게이지 100%로 설정
                var scooter = other.GetComponent<scooterCtrl>();
                if (scooter != null)
                {
                    scooter.SetBoostGaugeToMax();
                }
                break;
            case ItemType.Ink:
                // 먹물 효과 호출
                var uiManager = FindFirstObjectByType<UIManager>();
                if (uiManager != null)
                {
                    uiManager.ShowInkEffect(inkDuration);
                }
                break;
        }
        Debug.Log($"[ItemManager] 아이템 먹음! {itemType} {gameObject.name} 위치: {transform.position}");
        StartCoroutine(RespawnItemCoroutine());
    }

    // 10초 뒤에 같은 위치에 리스폰
    private IEnumerator RespawnItemCoroutine()
    {
        Vector3 respawnPosition = transform.position;
        Quaternion respawnRotation = transform.rotation;

        Debug.Log($"[ItemManager] 아이템 비활성화: {gameObject.name} 위치: {respawnPosition}");
        gameObject.SetActive(false);
        yield return new WaitForSeconds(5f);

        Debug.Log($"[ItemManager] 아이템 리스폰 시도: {gameObject.name} 위치: {respawnPosition}");
        GameObject newItem = Instantiate(itemPrefab, respawnPosition, respawnRotation);
        newItem.SetActive(true);
        Debug.Log($"[ItemManager] 아이템 리스폰 완료: {newItem.name} 위치: {respawnPosition}");

        Destroy(gameObject);
    }

    void Update()
    {
        // Y축 기준으로 초당 90도 회전 (속도는 원하는 값으로 조절)
        transform.Rotate(Vector3.up * 90f * Time.deltaTime);
    }
}
