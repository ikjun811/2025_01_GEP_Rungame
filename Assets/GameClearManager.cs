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
            Debug.LogError("���� �޴� ��ư�� GameClearManager�� �Ҵ���� �ʾҽ��ϴ�!"); //
        }

        // Ŭ���� �ð� ǥ��
        if (clearTimeText != null)
        {
            if (GameRoot.Instance != null)
            {
                float totalPlayTime = GameRoot.Instance.getPlayTime(); //

                // �ð��� ��:�� �������� ��ȯ
                int minutes = Mathf.FloorToInt(totalPlayTime / 60F);
                int seconds = Mathf.FloorToInt(totalPlayTime % 60F); // �Ǵ� totalPlayTime - minutes * 60
                string formattedTime = string.Format("{0:00}:{1:00}", minutes, seconds);

                clearTimeText.text = "Clear Time : " + formattedTime;
            }
            else
            {
                clearTimeText.text = "Clear Time : EMPTY";
                Debug.LogError("GameRoot �ν��Ͻ��� ã�� �� ���� Ŭ���� �ð��� ǥ���� �� �����ϴ�.");
            }
        }
        else
        {
            Debug.LogError("clearTimeText�� GameClearManager�� �Ҵ���� �ʾҽ��ϴ�!");
        }

        Cursor.lockState = CursorLockMode.None; 
        Cursor.visible = true; 
    }

    public void GoToMainMenu()
    {
        Debug.Log($"'{mainMenuSceneName}' ������ �̵��մϴ�.");
        SceneManager.LoadScene(mainMenuSceneName);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
