using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;


public class GameClearManager : MonoBehaviour
{

    public Button mainMenuButton;
    public string mainMenuSceneName = "MainMenu"; //
    public TextMeshProUGUI clearTimeText;

    // Start is called before the first frame update
    void Start()
    {
        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.AddListener(GoToMainMenu); //
        }
        else
        {
            Debug.LogError("메인 메뉴 버튼이 GameClearManager에 할당되지 않았습니다!"); //
        }

        // 클리어 시간 표시
        if (clearTimeText != null)
        {
            if (GameRoot.Instance != null)
            {
                float totalPlayTime = GameRoot.Instance.getPlayTime(); //

                // 시간을 분:초 형식으로 변환
                int minutes = Mathf.FloorToInt(totalPlayTime / 60F);
                int seconds = Mathf.FloorToInt(totalPlayTime % 60F); // 또는 totalPlayTime - minutes * 60
                string formattedTime = string.Format("{0:00}:{1:00}", minutes, seconds);

                clearTimeText.text = "Clear Time : " + formattedTime;
            }
            else
            {
                clearTimeText.text = "Clear Time : EMPTY";
                Debug.LogError("GameRoot 인스턴스를 찾을 수 없어 클리어 시간을 표시할 수 없습니다.");
            }
        }
        else
        {
            Debug.LogError("clearTimeText가 GameClearManager에 할당되지 않았습니다!");
        }

        Cursor.lockState = CursorLockMode.None; 
        Cursor.visible = true; 
    }

    public void GoToMainMenu()
    {
        Debug.Log($"'{mainMenuSceneName}' 씬으로 이동합니다.");
        SceneManager.LoadScene(mainMenuSceneName);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
