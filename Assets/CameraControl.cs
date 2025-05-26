using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CameraControl : MonoBehaviour
{
    private GameObject player = null;
    private Vector3 position_offset = Vector3.zero;

    private PlayerControl playerControl = null;

    public float smoothSpeed = 5.0f; //�÷��̾ ������� �ӵ�

    public string gameStageSceneName = "GameStage";


    // Start is called before the first frame update
    void Start()
    {
        if (SceneManager.GetActiveScene().name == gameStageSceneName)
        {
            AssignPlayerReferences();
        }

        // ��� ���� player�� Player ������Ʈ�� �Ҵ�
        this.player = GameObject.FindGameObjectWithTag("Player");
        // ī�޶� ��ġ�� �÷��̾� ��ġ�� ����
        this.position_offset = this.transform.position - this.player.transform.position;

        this.playerControl = this.player.GetComponent<PlayerControl>(); // ���� �� ���� ��������
    }

    void AssignPlayerReferences()
    {
        // ��� ���� player�� Player ������Ʈ�� �Ҵ�
        this.player = GameObject.FindGameObjectWithTag("Player");
        // ī�޶� ��ġ�� �÷��̾� ��ġ�� ����
        this.position_offset = this.transform.position - this.player.transform.position;

        this.playerControl = this.player.GetComponent<PlayerControl>(); // ���� �� ���� ��������
    }

    void LateUpdate()
    { // ��� ���� ������Ʈ�� Update() �޼��� ó�� �Ŀ� �ڵ����� ȣ��

        if (SceneManager.GetActiveScene().name != gameStageSceneName)
        {
            return;
        }

        if (player == null || playerControl == null)
        {
            AssignPlayerReferences(); // �÷��̾ �ٽ� ã�ƺ�
            if (player == null || playerControl == null) // ������ ���ٸ� �ߴ�
            {
                return;
            }
        }

        float targetX = this.player.transform.position.x + this.position_offset.x; //
        Vector3 desiredPosition = new Vector3(targetX, transform.position.y, transform.position.z);


        if (playerControl.IsPlayerDashing())
        {
            // ���� ��: ī�޶��� X�� ��ġ ������Ʈ�� ���� ���� (���� ��ġ�� ����)
            return;
        }
        else
        {
            // ���� ���� �ƴ� ��: �÷��̾ �ε巴�� ����
            transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
        }
        /*
        // ī�޶� ���� ��ġ�� new_position�� �Ҵ�
        Vector3 new_position = this.transform.position;
        // �÷��̾��� X��ǥ�� ���� ���� ���ؼ� new_position�� X�� ����
        new_position.x = this.player.transform.position.x + this.position_offset.x;
        // ī�޶� ��ġ�� ���ο� ��ġ�� ����
        this.transform.position = new_position;*/
    }

    // Update is called once per frame
    void Update()
    {

    }
}