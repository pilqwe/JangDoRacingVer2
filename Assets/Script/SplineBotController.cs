using UnityEngine;

public class SplineBotController : MonoBehaviour
{
    public Transform[] splinePoints; // 스플라인 경로 포인트들
    public float moveSpeed = 10f;
    private int currentPoint = 0;
    
    // 🆕 초기 위치와 회전 저장용
    private Vector3 initialPosition;
    private Quaternion initialRotation;

    void Start()
    {
        // 🆕 초기 위치와 회전 저장
        initialPosition = transform.position;
        initialRotation = transform.rotation;
    }

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

    /// <summary>
    /// 🆕 봇을 초기 상태로 리셋
    /// </summary>
    public void ResetToInitialState()
    {
        Debug.Log($"🤖 {gameObject.name} 봇 초기 상태로 리셋!");
        
        // 위치와 회전 초기화
        transform.position = initialPosition;
        transform.rotation = initialRotation;
        
        // 스플라인 진행도 초기화
        currentPoint = 0;
        
        Debug.Log($"✅ {gameObject.name} 봇 리셋 완료! 위치: {transform.position}");
    }
}
