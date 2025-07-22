using UnityEngine;

public class MinimapFollow : MonoBehaviour
{
    public Transform target;

    void LateUpdate()
    {
        Vector3 newPos = target.position;
        newPos.y = transform.position.y; // 고도 유지
        transform.position = newPos;

        // 회전도 플레이어에 맞춰주려면 아래 코드 활성화
        transform.rotation = Quaternion.Euler(90f, target.eulerAngles.y, 0f);
    }
}