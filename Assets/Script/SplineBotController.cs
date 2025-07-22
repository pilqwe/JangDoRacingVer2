using UnityEngine;

public class SplineBotController : MonoBehaviour
{
    public Transform[] splinePoints; // 스플라인 경로 포인트들
    public float moveSpeed = 10f;
    private int currentPoint = 0;

    void Update()
    {
        if (splinePoints == null || splinePoints.Length == 0) return;

        Transform target = splinePoints[currentPoint];
        Vector3 dir = (target.position - transform.position).normalized;
        transform.position += dir * moveSpeed * Time.deltaTime;
        transform.forward = dir;

        if (Vector3.Distance(transform.position, target.position) < 0.5f)
        {
            currentPoint++;
            if (currentPoint >= splinePoints.Length)
            {
                currentPoint = 0; // 한 바퀴 돌면 처음으로
                // 랩 체크는 GameManager에 알리기
            }
        }
    }
}
