using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameOverManager : MonoBehaviour
{
    [Header("UI 버튼 연결")]
    public Button retryButton;
    public Button mainMenuButton;

    [Header("씬 이름 설정")]
    public string gameStageSceneName = "GameStage"; // 실제 게임 스테이지 씬 이름
    public string mainMenuSceneName = "MainMenu";

    // Start is called before the first frame update
    void Start()
    {
        if (retryButton != null)
        {
            retryButton.onClick.AddListener(RetryGame);
        }
        else
        {
            Debug.LogError("GameOverManager: RetryButton이 연결되지 않았습니다!");
        }

        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.AddListener(GoToMainMenu);
        }
        else
        {
            Debug.LogError("GameOverManager: MainMenuButton이 연결되지 않았습니다!");
        }

        // 게임 오버 화면에서는 마우스 커서가 보이도록 설정
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void RetryGame()
    {
        Debug.Log($"GameOverManager: '{gameStageSceneName}' 씬에서 게임 재시도.");
        if (GameRoot.Instance != null)
        {
            // 게임을 처음부터 (스테이지 0부터) 다시 시작합니다.
            GameRoot.Instance.PrepareForNewGameSession();
        }
        SceneManager.LoadScene(gameStageSceneName);
    }

    public void GoToMainMenu()
    {
        Debug.Log($"GameOverManager: '{mainMenuSceneName}' 씬으로 이동.");
        // 메인 메뉴에서 "새 게임 시작" 시 GameRoot 상태가 초기화되므로,
        // 여기서는 별도의 GameRoot 초기화 호출이 필요 없을 수 있습니다.
        SceneManager.LoadScene(mainMenuSceneName);
    }

}
