using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RedWallMovement : MonoBehaviour
{
    private float speed;
    private Vector3 direction;
    private bool isInitialized = false;
    private Transform playerTransformForDespawn;
    public float damageToPlayer = 30f;


    public void Initialize(Vector3 dir, float spd)
    {
        this.direction = dir.normalized;
        this.speed = spd;
        this.isInitialized = true;

        if (this.direction != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(this.direction);
        }

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) playerTransformForDespawn = playerObj.transform;
    }


    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (!isInitialized) return;

        transform.Translate(direction * speed * Time.deltaTime, Space.World);

        // 화면 왼쪽으로 너무 멀리 벗어나면 자동 파괴 (플레이어 기준)
        if (playerTransformForDespawn != null && transform.position.x < playerTransformForDespawn.position.x - 50f) // 50f는 예시 거리
        {
            Destroy(gameObject);
        }
        // 플레이어를 못 찾았을 경우 대비, 절대 좌표 기준으로도 파괴 (선택적)
        else if (playerTransformForDespawn == null && transform.position.x < -100f) // -100f는 예시 좌표
        {
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerControl playerControl = other.GetComponent<PlayerControl>();
            if (playerControl != null)
            {
                // 돌진 상태를 확인하지 않고 바로 데미지를 줍니다.
                playerControl.TakeDamage(damageToPlayer);
            }
            Destroy(gameObject); // 벽은 플레이어와 충돌 후 파괴
        }
    }
}