using UnityEngine;

public class BoostItem : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            var scooter = other.GetComponent<scooterCtrl>();
            if (scooter != null)
                scooter.SetBoostGaugeToMax(); // 또는 부스트 게이지를 100%로 만드는 함수
            Destroy(gameObject);
        }
    }
}
