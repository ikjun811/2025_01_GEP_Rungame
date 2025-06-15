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



    void Awake()
    {
        // 1. �� ī�޶� ���� ī�޶� ������ �ϵ��� �����մϴ�.
        // �� �ڵ�� �� ī�޶� �׻� MainCamera �±׸� ������ �����մϴ�.
        if (gameObject.tag != "MainCamera")
        {
            gameObject.tag = "MainCamera";
            Debug.LogWarning($"ī�޶� '{gameObject.name}'�� �±׸� 'MainCamera'�� ���� �����߽��ϴ�.");
        }

        // 2. ī�޶��� Clear Flags�� Skybox�� ���� �����մϴ�.
        // �̰��� ����� ������ �ʴ� ������ �������� ������ �� �ֽ��ϴ�.
        Camera cam = GetComponent<Camera>();
        if (cam.clearFlags != CameraClearFlags.Skybox)
        {
            cam.clearFlags = CameraClearFlags.Skybox;
            Debug.LogWarning($"ī�޶� '{gameObject.name}'�� Clear Flags�� 'Skybox'�� ���� �����߽��ϴ�.");
        }

        // 3. Culling Mask�� Everything���� ���� �����մϴ� (��� ���̾ ������).
        // Ư�� ���̾ ������ �� ���̴� ������ �����մϴ�.
        // -1�� ��Ʈ����ũ�� ��� ��Ʈ�� 1���� �ǹ��ϸ�, 'Everything'�� �����ϴ�.
        if (cam.cullingMask != -1)
        {
            cam.cullingMask = -1;
            Debug.LogWarning($"ī�޶� '{gameObject.name}'�� Culling Mask�� 'Everything'���� ���� �����߽��ϴ�.");
        }

        // �� �ε� �� �÷��̾� ������ �ٽ� �ϵ��� �̺�Ʈ�� �����մϴ�.
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // ���� �������� ���� ���� �÷��̾� ������ ã���ϴ�.
        if (scene.name == gameStageSceneName)
        {
            AssignPlayerReferences();
        }
    }

    /*
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
    */
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