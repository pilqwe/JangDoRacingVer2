using UnityEngine;

public class SplineBotController : MonoBehaviour
{
    public Transform[] splinePoints; // 스플라인 경로 포인트들
    public float moveSpeed = 10f;
    private int currentPoint = 0;

    void Update()
    {
        if (splinePoints == null || splinePoints.Length == 0)
        {
            Debug.LogWarning($"{gameObject.name}: splinePoints가 비어있거나 연결되지 않았습니다.");
            return;
        }

        Transform target = splinePoints[currentPoint];
        Vector3 dir = (target.position - transform.position).normalized;
        Debug.Log($"{gameObject.name}: 현재 포인트 {currentPoint}, dir={dir}, target={target.name}");

        transform.position += dir * moveSpeed * Time.deltaTime;
        if (dir != Vector3.zero)
            transform.forward = dir;

        if (Vector3.Distance(transform.position, target.position) < 0.5f)
        {
            Debug.Log($"{gameObject.name}: {target.name}에 도착, 다음 포인트로 이동");
            currentPoint++;
            if (currentPoint >= splinePoints.Length)
            {
                Debug.Log($"{gameObject.name}: 한 바퀴 완료, 처음으로!");
                currentPoint = 0; // 한 바퀴 돌면 처음으로
                // 랩 체크는 GameManager에 알리기
            }
        }
    }
}
