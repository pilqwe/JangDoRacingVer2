using UnityEngine;

public class ItemManager : MonoBehaviour
{
    public enum ItemType { Boost, Ink }
    [Header("아이템 타입 설정")]
    public ItemType itemType = ItemType.Boost;
    [Header("먹물 효과 지속 시간 (초)")]
    public float inkDuration = 3f;

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
        Destroy(gameObject);
    }

    void Update()
    {
        // Y축 기준으로 초당 90도 회전 (속도는 원하는 값으로 조절)
        transform.Rotate(Vector3.up * 90f * Time.deltaTime);
    }
}
