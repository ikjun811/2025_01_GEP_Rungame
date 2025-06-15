using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowPlayer : MonoBehaviour
{
  
    private Transform playerTransform;

    void Start()
    {
      
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
        else
        {
            Debug.LogError("FollowPlayer: 'Player' 태그를 가진 오브젝트를 찾을 수 없습니다!");
           
            this.enabled = false;
        }
    }

   
    void LateUpdate()
    {
        if (playerTransform == null) return;

      
        Vector3 newPosition = transform.position;

        
        newPosition.x = playerTransform.position.x;

       
        transform.position = newPosition;
    }
}
