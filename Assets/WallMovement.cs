using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallMovement : MonoBehaviour
{

    private float speed;
    private Vector3 direction;
    private bool isInitialized = false;


    // 화면 밖으로 나가면 자동 파괴될 X 좌표
    private float despawnXPosition = -50f;
    private Transform playerTransformForDespawn;

    public float damageToPlayer = 20f;

    public void Initialize(Vector3 dir, float spd)
    {
        this.direction = dir.normalized; // 방향 벡터 정규화
        this.speed = spd;
        this.isInitialized = true;

        // 벽이 진행 방향을 바라보도록 회전 (선택적)
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
        transform.Translate(direction * speed * Time.deltaTime, Space.World);

        if (playerTransformForDespawn != null && transform.position.x < playerTransformForDespawn.position.x - 30f)
        {
            Destroy(gameObject);
        }
        else if (playerTransformForDespawn == null && transform.position.x < despawnXPosition)
        {
            // 플레이어를 못찾았을 경우 대비 고정 좌표 기반 파괴
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
                // 플레이어가 돌진 중인지 확인 (PlayerControl에 IsPlayerDashing() 메서드가 있다고 가정)
                if (playerControl.IsPlayerDashing())
                {
                    Debug.Log("플레이어가 돌진으로 벽 돌파!");
                    playerControl.CollectStone(); // 돌멩이 획득
                    Destroy(gameObject); // 벽 파괴 (데미지 없음)
                }
                else
                {
                    Debug.Log("플레이어가 벽에 충돌!");
                    playerControl.TakeDamage(damageToPlayer); // 플레이어 데미지
                    Destroy(gameObject); // 벽 파괴
                }
            }
        }
    }

}
