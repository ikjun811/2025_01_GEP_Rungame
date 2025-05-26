using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CameraControl : MonoBehaviour
{
    private GameObject player = null;
    private Vector3 position_offset = Vector3.zero;

    private PlayerControl playerControl = null;

    public float smoothSpeed = 5.0f; //플레이어를 따라잡는 속도

    public string gameStageSceneName = "GameStage";


    // Start is called before the first frame update
    void Start()
    {
        if (SceneManager.GetActiveScene().name == gameStageSceneName)
        {
            AssignPlayerReferences();
        }

        // 멤버 변수 player에 Player 오브젝트를 할당
        this.player = GameObject.FindGameObjectWithTag("Player");
        // 카메라 위치와 플레이어 위치의 차이
        this.position_offset = this.transform.position - this.player.transform.position;

        this.playerControl = this.player.GetComponent<PlayerControl>(); // 시작 시 참조 가져오기
    }

    void AssignPlayerReferences()
    {
        // 멤버 변수 player에 Player 오브젝트를 할당
        this.player = GameObject.FindGameObjectWithTag("Player");
        // 카메라 위치와 플레이어 위치의 차이
        this.position_offset = this.transform.position - this.player.transform.position;

        this.playerControl = this.player.GetComponent<PlayerControl>(); // 시작 시 참조 가져오기
    }

    void LateUpdate()
    { // 모든 게임 오브젝트의 Update() 메서드 처리 후에 자동으로 호출

        if (SceneManager.GetActiveScene().name != gameStageSceneName)
        {
            return;
        }

        if (player == null || playerControl == null)
        {
            AssignPlayerReferences(); // 플레이어를 다시 찾아봄
            if (player == null || playerControl == null) // 여전히 없다면 중단
            {
                return;
            }
        }

        float targetX = this.player.transform.position.x + this.position_offset.x; //
        Vector3 desiredPosition = new Vector3(targetX, transform.position.y, transform.position.z);


        if (playerControl.IsPlayerDashing())
        {
            // 돌진 중: 카메라의 X축 위치 업데이트를 하지 않음 (현재 위치에 고정)
            return;
        }
        else
        {
            // 돌진 중이 아닐 때: 플레이어를 부드럽게 따라감
            transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
        }
        /*
        // 카메라 현재 위치를 new_position에 할당
        Vector3 new_position = this.transform.position;
        // 플레이어의 X좌표에 차이 값을 더해서 new_position의 X에 대입
        new_position.x = this.player.transform.position.x + this.position_offset.x;
        // 카메라 위치를 새로운 위치로 갱신
        this.transform.position = new_position;*/
    }

    // Update is called once per frame
    void Update()
    {

    }
}