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
            Debug.LogError("StartGameButton이 MainMenuManager에 할당되지 않았습니다!");
        }

        howToPlayButton.onClick.AddListener(ShowHowToPlayPanel);
        infoButton.onClick.AddListener(ShowInfoPanel);

        howToPlayPanel.SetActive(true);
        infoPanel.SetActive(false);

        // 메인 메뉴에서는 마우스 커서가 항상 보이도록 설정
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void StartGame()
    {
        if (GameRoot.Instance != null)
        {
            GameRoot.Instance.PrepareForNewGameSession(); // GameRoot에 이 함수를 만들어야 합니다.
        }
      

        Debug.Log($"새 게임 시작. '{gameStageSceneName}' 씬으로 전환합니다.");
        SceneManager.LoadScene(gameStageSceneName);
    }

    public void ShowHowToPlayPanel()
    {
        infoPanel.SetActive(false);

        howToPlayPanel.SetActive(true);

        Debug.Log("MainMenuManager: '게임 방법' 패널 표시");
    }

    // '정보' 패널을 보여주는 함수
    public void ShowInfoPanel()
    {
        howToPlayPanel.SetActive(false);

        infoPanel.SetActive(true);

        Debug.Log("MainMenuManager: '정보' 패널 표시");
    }



    // Update is called once per frame
    void Update()
    {
        
    }
}
