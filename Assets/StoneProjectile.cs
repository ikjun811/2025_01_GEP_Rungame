using System.Collections;
using System.Collections.Generic;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoneProjectile : MonoBehaviour
{
    public float lifeTime = 3.0f; // 일정 시간 후 자동 파괴
    public float damageToWeakPoint = 1f; // 약점 타격 시 보스에게 줄 데미지

    // Start is called before the first frame update
    void Start()
    {
        Destroy(gameObject, lifeTime); // 수명 설정
    }


    void OnTriggerEnter(Collider other) // 또는 OnCollisionEnter
    {
        if (other.CompareTag("BossWeakPoint")) // 보스 약점 태그와 충돌 시
        {
            Debug.Log("돌멩이가 보스 약점에 명중!");
            BossControl bossControl = other.GetComponentInParent<BossControl>(); // 약점이 보스의 자식일 경우
            // 또는 BossWeakPoint 스크립트가 있다면:
            // BossWeakPoint weakPoint = other.GetComponent<BossWeakPoint>();
            // weakPoint?.HitByStone(damageToWeakPoint); // BossWeakPoint에 이런 함수 필요

            if (bossControl != null)
            {
                bossControl.TakeDamage(damageToWeakPoint); // 보스에게 데미지 전달
            }

            // if (impactEffectPrefab != null) Instantiate(impactEffectPrefab, transform.position, Quaternion.identity);
            Destroy(gameObject); // 돌멩이 파괴
        }

        if (other.CompareTag("Boss"))
        {
            Debug.Log("돌멩이가 보스 몸체에 명중하여 파괴됩니다.");

            // if (impactEffectPrefab_Body != null) Instantiate(impactEffectPrefab_Body, transform.position, Quaternion.identity);
            Destroy(gameObject); // 돌멩이 파괴
            return; // 충돌 처리 완료
        }

    }

    // Update is called once per frame
    void Update()
    {

    }
}