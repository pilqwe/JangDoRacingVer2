using UnityEngine;
using System.Collections;

public class CableCarController : MonoBehaviour
{
    private Transform[] waypoints;
    private float moveSpeed;
    private bool swayAnimation;
    private float swayAmount;
    private float swaySpeed;
    private CableCarSystem parentSystem;

    private int currentWaypointIndex = 0;
    private float journeyLength;
    private float journeyTime;
    private Vector3 originalPosition;
    private bool isMoving = false;

    // 흔들림 애니메이션용 변수
    private float swayTimer = 0f;
    private Vector3 initialLocalPosition;

    public void Initialize(Transform[] waypoints, float speed, bool sway, float swayAmt, float swaySpd, CableCarSystem system)
    {
        this.waypoints = waypoints;
        this.moveSpeed = speed;
        this.swayAnimation = sway;
        this.swayAmount = swayAmt;
        this.swaySpeed = swaySpd;
        this.parentSystem = system;

        initialLocalPosition = transform.localPosition;

        if (waypoints.Length > 1)
        {
            StartMovement();
        }
    }

    void Update()
    {
        if (isMoving)
        {
            MoveBetweenWaypoints();
        }

        if (swayAnimation)
        {
            ApplySwayAnimation();
        }
    }

    private void StartMovement()
    {
        if (currentWaypointIndex < waypoints.Length - 1)
        {
            // 현재 웨이포인트에서 다음 웨이포인트까지의 거리 계산
            journeyLength = Vector3.Distance(waypoints[currentWaypointIndex].position,
                                           waypoints[currentWaypointIndex + 1].position);
            journeyTime = 0f;
            isMoving = true;

            // 다음 웨이포인트 방향으로 회전
            Vector3 direction = waypoints[currentWaypointIndex + 1].position - waypoints[currentWaypointIndex].position;
            if (direction != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(direction);
            }
        }
        else
        {
            // 마지막 웨이포인트에 도달했으면 제거
            if (parentSystem != null)
            {
                parentSystem.DespawnCableCar(gameObject);
            }
        }
    }

    private void MoveBetweenWaypoints()
    {
        // 이동 시간 업데이트
        journeyTime += Time.deltaTime * moveSpeed;
        float fractionOfJourney = journeyTime / journeyLength;

        if (fractionOfJourney <= 1f)
        {
            // 두 웨이포인트 사이를 부드럽게 이동
            Vector3 startPos = waypoints[currentWaypointIndex].position;
            Vector3 endPos = waypoints[currentWaypointIndex + 1].position;

            // 곡선 이동을 위한 보간 (선택적)
            transform.position = Vector3.Lerp(startPos, endPos, fractionOfJourney);
        }
        else
        {
            // 다음 웨이포인트에 도달
            currentWaypointIndex++;

            if (currentWaypointIndex < waypoints.Length - 1)
            {
                StartMovement();
            }
            else
            {
                isMoving = false;
                // 경로 끝에 도달하면 제거
                StartCoroutine(DelayedDespawn(1f));
            }
        }
    }

    private void ApplySwayAnimation()
    {
        swayTimer += Time.deltaTime * swaySpeed;

        // 사인파를 이용한 부드러운 좌우 흔들림
        float swayX = Mathf.Sin(swayTimer) * swayAmount;
        float swayZ = Mathf.Sin(swayTimer * 0.7f) * swayAmount * 0.5f; // 약간의 앞뒤 흔들림도 추가

        Vector3 swayOffset = new Vector3(swayX, 0, swayZ);

        // 로컬 좌표계 기준으로 흔들림 적용
        transform.localPosition = initialLocalPosition + transform.TransformDirection(swayOffset);
    }

    private IEnumerator DelayedDespawn(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (parentSystem != null)
        {
            parentSystem.DespawnCableCar(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // 현재 진행 상황을 외부에서 확인할 수 있는 프로퍼티
    public float Progress
    {
        get
        {
            if (waypoints == null || waypoints.Length <= 1) return 1f;
            return (float)currentWaypointIndex / (waypoints.Length - 1);
        }
    }

    public bool IsMoving => isMoving;
    public int CurrentWaypointIndex => currentWaypointIndex;

    // 긴급 정지 (필요시 사용)
    public void Stop()
    {
        isMoving = false;
    }

    // 움직임 재개
    public void Resume()
    {
        if (currentWaypointIndex < waypoints.Length - 1)
        {
            isMoving = true;
        }
    }

    // 속도 변경
    public void SetSpeed(float newSpeed)
    {
        moveSpeed = newSpeed;
    }

    void OnDrawGizmos()
    {
        // 현재 목표 웨이포인트 표시
        if (waypoints != null && currentWaypointIndex < waypoints.Length - 1)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, waypoints[currentWaypointIndex + 1].position);
            Gizmos.DrawWireSphere(waypoints[currentWaypointIndex + 1].position, 0.5f);
        }
    }
}
