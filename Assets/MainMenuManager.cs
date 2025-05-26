using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    public Button startGameButton; // Inspector���� "���� ����" ��ư ����
    public string gameStageSceneName = "GameStageScene";

    [Header("���� �г� UI")]
    public GameObject howToPlayPanel;  
    public GameObject infoPanel;        
    public Button howToPlayButton;      
    public Button infoButton;


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

        howToPlayButton.onClick.AddListener(ShowHowToPlayPanel);
        infoButton.onClick.AddListener(ShowInfoPanel);

        howToPlayPanel.SetActive(true);
        infoPanel.SetActive(false);

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
      

        Debug.Log($"�� ���� ����. '{gameStageSceneName}' ������ ��ȯ�մϴ�.");
        SceneManager.LoadScene(gameStageSceneName);
    }

    public void ShowHowToPlayPanel()
    {
        infoPanel.SetActive(false);

        howToPlayPanel.SetActive(true);

        Debug.Log("MainMenuManager: '���� ���' �г� ǥ��");
    }

    // '����' �г��� �����ִ� �Լ�
    public void ShowInfoPanel()
    {
        howToPlayPanel.SetActive(false);

        infoPanel.SetActive(true);

        Debug.Log("MainMenuManager: '����' �г� ǥ��");
    }



    // Update is called once per frame
    void Update()
    {
        
    }
}
