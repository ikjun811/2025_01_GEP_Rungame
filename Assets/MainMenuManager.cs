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
    public GameObject storyPanel;
    public GameObject howToPlayPanel;  
    public GameObject infoPanel;
    public Button storyButton;
    public Button howToPlayButton;      
    public Button infoButton;



    // Start is called before the first frame update
    void Start()
    {
        if (startGameButton != null)
            startGameButton.onClick.AddListener(StartGame);
        else
            Debug.LogError("StartGameButton�� MainMenuManager�� �Ҵ���� �ʾҽ��ϴ�!");

        if (storyButton != null) storyButton.onClick.AddListener(ShowStoryPanel);
        if (howToPlayButton != null) howToPlayButton.onClick.AddListener(ShowHowToPlayPanel);
        if (infoButton != null) infoButton.onClick.AddListener(ShowInfoPanel);

        howToPlayPanel.SetActive(false);
        infoPanel.SetActive(false);
        storyPanel.SetActive(false);

        // ���� �޴������� ���콺 Ŀ���� �׻� ���̵��� ����
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void StartGame()
    {
        if (GameRoot.Instance != null)
        {
            GameRoot.Instance.PrepareForNewGameSession();
        }
        SceneManager.LoadScene(gameStageSceneName);
    }

    public void ShowStoryPanel()
    {
        bool isActive = storyPanel.activeSelf;
        storyPanel.SetActive(!isActive);

        if (!isActive) 
        {
            howToPlayPanel.SetActive(false);
            infoPanel.SetActive(false);
        }

        Debug.Log("MainMenuManager: '���丮' �г� ǥ��");
    }

    public void ShowHowToPlayPanel()
    {

        bool isActive = howToPlayPanel.activeSelf;
        howToPlayPanel.SetActive(!isActive);

        // ���丮 �г��� ������ �������� �ٸ� �гε��� ��� ����.
        if (!isActive) // !isActive�� true��� ���� �г��� �����ٴ� �ǹ�
        {
            storyPanel.SetActive(false);
            infoPanel.SetActive(false);
        }

      

        Debug.Log("MainMenuManager: '���� ���' �г� ǥ��");
    }

    // '����' �г��� �����ִ� �Լ�
    public void ShowInfoPanel()
    {

        bool isActive = infoPanel.activeSelf;
        infoPanel.SetActive(!isActive);

        // ���丮 �г��� ������ �������� �ٸ� �гε��� ��� ����.
        if (!isActive) // !isActive�� true��� ���� �г��� �����ٴ� �ǹ�
        {
            storyPanel.SetActive(false);
            howToPlayPanel.SetActive(false);
        }


        Debug.Log("MainMenuManager: '����' �г� ǥ��");
    }


    // Update is called once per frame
    void Update()
    {
        
    }
}
