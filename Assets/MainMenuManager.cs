using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    public Button startGameButton; // Inspector���� "���� ����" ��ư ����
    public string gameStageSceneName = "GameStageScene";


    // Start is called before the first frame update
    void Start()
    {
        if (startGameButton != null)
        {
            startGameButton.onClick.AddListener(StartGame);
        }
        else
        {
            Debug.LogError("StartGameButton�� MainMenuManager�� �Ҵ���� �ʾҽ��ϴ�!");
        }

        // ���� �޴������� ���콺 Ŀ���� �׻� ���̵��� ����
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void StartGame()
    {
        if (GameRoot.Instance != null)
        {
            GameRoot.Instance.PrepareForNewGameSession(); // GameRoot�� �� �Լ��� ������ �մϴ�.
        }
        // GameRoot �ν��Ͻ��� ���ٸ� (�� ù ���� �� �ٷ� ���� ���� ��),
        // GameStageScene�� �ε�Ǹ鼭 GameRoot�� Awake()�� ȣ��Ǿ� ó������ ���� �� �ʱ�ȭ�˴ϴ�.

        Debug.Log($"�� ���� ����. '{gameStageSceneName}' ������ ��ȯ�մϴ�.");
        SceneManager.LoadScene(gameStageSceneName);
    }



    // Update is called once per frame
    void Update()
    {
        
    }
}
