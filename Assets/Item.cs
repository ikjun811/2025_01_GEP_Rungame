using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum ItemType
{
    HealthPack, // ü�� ȸ�� ������
    StonePack,   // ������ ȸ�� ������
           Shield //�� ������
}

public class Item : MonoBehaviour
{

    public ItemType itemType; // Inspector���� ������ ������ ����
    public float healthRestoreAmount = 25f; // ü�� ȸ�� �������� ��� ȸ����

    // �÷��̾�� �浹(Trigger) �� ȣ��� �Լ�
    void OnTriggerEnter(Collider other)
    {
        // �浹�� ������Ʈ�� "Player" �±׸� ������ �ִ��� Ȯ��
        if (other.CompareTag("Player"))
        {
            PlayerControl playerControl = other.GetComponent<PlayerControl>();
            if (playerControl != null)
            {
                CollectItem(playerControl); // �÷��̾�� ������ ȿ�� ����
            }
            Destroy(gameObject); // ������ ������Ʈ �ı�
        }
    }

    // ������ ȿ���� �÷��̾�� �����ϴ� �Լ�
    void CollectItem(PlayerControl player)
    {
        Debug.Log($"������ ȹ��: {itemType}");

        switch (itemType)
        {
            case ItemType.HealthPack:
                player.RestoreSpecificHealth(healthRestoreAmount); // PlayerControl�� �� �Լ� �߰� �ʿ�
                break;
            case ItemType.StonePack:
                player.CollectStone(); // PlayerControl�� ���� CollectStone() �Լ� ���
                break;
            case ItemType.Shield: // 
                player.ActivateShield(); // 
                break;
        }
       
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
