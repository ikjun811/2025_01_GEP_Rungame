using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallMovement : MonoBehaviour
{

    private float speed;
    private Vector3 direction;
    private bool isInitialized = false;
    private Transform playerTransformForDespawn;

    public float damageToPlayer = 20f;


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

        // ȭ�� �������� �ʹ� �ָ� ����� �ڵ� �ı� (�÷��̾� ����)
        if (playerTransformForDespawn != null && transform.position.x < playerTransformForDespawn.position.x - 50f) // 50f�� ���� �Ÿ�
        {
            Destroy(gameObject);
        }
        // �÷��̾ �� ã���� ��� ���, ���� ��ǥ �������ε� �ı� (������)
        else if (playerTransformForDespawn == null && transform.position.x < -100f) // -100f�� ���� ��ǥ
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
                if (playerControl.IsPlayerDashing()) // �÷��̾ ���� ���̸�
                {
                    playerControl.CollectStone(); // ������ ȹ��
                }
                else // ���� ���� �ƴϸ� ������
                {
                    playerControl.TakeDamage(damageToPlayer);
                }
            }
            Destroy(gameObject); // ���� �÷��̾�� ��ȣ�ۿ� �� �ı�
        }
    }
}
