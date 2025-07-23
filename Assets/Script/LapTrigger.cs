using UnityEngine;

public class LapTrigger : MonoBehaviour
{
    // GameManager에 랩 체크 알림
    private void OnTriggerEnter(Collider other)
    {
        // 플레이어만 체크 (태그 또는 컴포넌트로 구분)
        if (other.CompareTag("Player"))
        {
            RacingGameManager.Instance.OnLapTrigger();
        }
    }
}
