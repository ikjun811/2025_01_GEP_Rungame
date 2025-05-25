using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallMovement : MonoBehaviour
{

    private float speed;
    private Vector3 direction;
    private bool isInitialized = false;


    // ȭ�� ������ ������ �ڵ� �ı��� X ��ǥ
    private float despawnXPosition = -50f;
    private Transform playerTransformForDespawn;

    public float damageToPlayer = 20f;

    public void Initialize(Vector3 dir, float spd)
    {
        this.direction = dir.normalized; // ���� ���� ����ȭ
        this.speed = spd;
        this.isInitialized = true;

        // ���� ���� ������ �ٶ󺸵��� ȸ�� (������)
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
            // �÷��̾ ��ã���� ��� ��� ���� ��ǥ ��� �ı�
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
                // �÷��̾ ���� ������ Ȯ�� (PlayerControl�� IsPlayerDashing() �޼��尡 �ִٰ� ����)
                if (playerControl.IsPlayerDashing())
                {
                    Debug.Log("�÷��̾ �������� �� ����!");
                    playerControl.CollectStone(); // ������ ȹ��
                    Destroy(gameObject); // �� �ı� (������ ����)
                }
                else
                {
                    Debug.Log("�÷��̾ ���� �浹!");
                    playerControl.TakeDamage(damageToPlayer); // �÷��̾� ������
                    Destroy(gameObject); // �� �ı�
                }
            }
        }
    }

}
