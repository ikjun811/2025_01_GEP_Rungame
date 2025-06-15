using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum ItemType
{
    HealthPack, // 체력 회복 아이템
    StonePack,   // 돌멩이 회복 아이템
           Shield //방어막 아이템
}

public class Item : MonoBehaviour
{

    public ItemType itemType; // Inspector에서 설정할 아이템 종류
    public float healthRestoreAmount = 25f; // 체력 회복 아이템일 경우 회복량

    // 플레이어와 충돌(Trigger) 시 호출될 함수
    void OnTriggerEnter(Collider other)
    {
        // 충돌한 오브젝트가 "Player" 태그를 가지고 있는지 확인
        if (other.CompareTag("Player"))
        {
            PlayerControl playerControl = other.GetComponent<PlayerControl>();
            if (playerControl != null)
            {
                CollectItem(playerControl); // 플레이어에게 아이템 효과 적용
            }
            Destroy(gameObject); // 아이템 오브젝트 파괴
        }
    }

    // 아이템 효과를 플레이어에게 적용하는 함수
    void CollectItem(PlayerControl player)
    {
        Debug.Log($"아이템 획득: {itemType}");

        switch (itemType)
        {
            case ItemType.HealthPack:
                player.RestoreSpecificHealth(healthRestoreAmount); // PlayerControl에 이 함수 추가 필요
                break;
            case ItemType.StonePack:
                player.CollectStone(); // PlayerControl의 기존 CollectStone() 함수 사용
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
