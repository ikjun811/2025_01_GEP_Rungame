using System.Collections;
using System.Collections.Generic;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoneProjectile : MonoBehaviour
{
    public float lifeTime = 3.0f; // ���� �ð� �� �ڵ� �ı�
    public float damageToWeakPoint = 1f; // ���� Ÿ�� �� �������� �� ������

    // Start is called before the first frame update
    void Start()
    {
        Destroy(gameObject, lifeTime); // ���� ����
    }


    void OnTriggerEnter(Collider other) // �Ǵ� OnCollisionEnter
    {
        if (other.CompareTag("BossWeakPoint")) // ���� ���� �±׿� �浹 ��
        {
            Debug.Log("�����̰� ���� ������ ����!");
            BossControl bossControl = other.GetComponentInParent<BossControl>(); // ������ ������ �ڽ��� ���
            // �Ǵ� BossWeakPoint ��ũ��Ʈ�� �ִٸ�:
            // BossWeakPoint weakPoint = other.GetComponent<BossWeakPoint>();
            // weakPoint?.HitByStone(damageToWeakPoint); // BossWeakPoint�� �̷� �Լ� �ʿ�

            if (bossControl != null)
            {
                bossControl.TakeDamage(damageToWeakPoint); // �������� ������ ����
            }

            // if (impactEffectPrefab != null) Instantiate(impactEffectPrefab, transform.position, Quaternion.identity);
            Destroy(gameObject); // ������ �ı�
        }

        if (other.CompareTag("Boss"))
        {
            Debug.Log("�����̰� ���� ��ü�� �����Ͽ� �ı��˴ϴ�.");

            // if (impactEffectPrefab_Body != null) Instantiate(impactEffectPrefab_Body, transform.position, Quaternion.identity);
            Destroy(gameObject); // ������ �ı�
            return; // �浹 ó�� �Ϸ�
        }

    }

    // Update is called once per frame
    void Update()
    {

    }
}