using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    public Button startGameButton; // Inspector에서 "게임 시작" 버튼 연결
    public string gameStageSceneName = "GameStageScene";

    [Header("정보 패널 UI")]
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
            Debug.LogError("StartGameButton이 MainMenuManager에 할당되지 않았습니다!");

        if (storyButton != null) storyButton.onClick.AddListener(ShowStoryPanel);
        if (howToPlayButton != null) howToPlayButton.onClick.AddListener(ShowHowToPlayPanel);
        if (infoButton != null) infoButton.onClick.AddListener(ShowInfoPanel);

        howToPlayPanel.SetActive(false);
        infoPanel.SetActive(false);
        storyPanel.SetActive(false);

        // 메인 메뉴에서는 마우스 커서가 항상 보이도록 설정
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

        Debug.Log("MainMenuManager: '스토리' 패널 표시");
    }

    public void ShowHowToPlayPanel()
    {

        bool isActive = howToPlayPanel.activeSelf;
        howToPlayPanel.SetActive(!isActive);

        // 스토리 패널이 켜지는 순간에는 다른 패널들을 모두 끈다.
        if (!isActive) // !isActive가 true라는 것은 패널이 켜졌다는 의미
        {
            storyPanel.SetActive(false);
            infoPanel.SetActive(false);
        }

      

        Debug.Log("MainMenuManager: '게임 방법' 패널 표시");
    }

    // '정보' 패널을 보여주는 함수
    public void ShowInfoPanel()
    {

        bool isActive = infoPanel.activeSelf;
        infoPanel.SetActive(!isActive);

        // 스토리 패널이 켜지는 순간에는 다른 패널들을 모두 끈다.
        if (!isActive) // !isActive가 true라는 것은 패널이 켜졌다는 의미
        {
            storyPanel.SetActive(false);
            howToPlayPanel.SetActive(false);
        }


        Debug.Log("MainMenuManager: '정보' 패널 표시");
    }


    // Update is called once per frame
    void Update()
    {
        
    }
}
