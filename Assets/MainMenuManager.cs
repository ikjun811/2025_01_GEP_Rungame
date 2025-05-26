using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    public Button startGameButton; // Inspector에서 "게임 시작" 버튼 연결
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
            Debug.LogError("StartGameButton이 MainMenuManager에 할당되지 않았습니다!");
        }

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
        // GameRoot 인스턴스가 없다면 (앱 첫 실행 후 바로 게임 시작 시),
        // GameStageScene이 로드되면서 GameRoot의 Awake()가 호출되어 처음으로 생성 및 초기화됩니다.

        Debug.Log($"새 게임 시작. '{gameStageSceneName}' 씬으로 전환합니다.");
        SceneManager.LoadScene(gameStageSceneName);
    }



    // Update is called once per frame
    void Update()
    {
        
    }
}
