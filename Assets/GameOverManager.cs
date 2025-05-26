using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameOverManager : MonoBehaviour
{
    [Header("UI ��ư ����")]
    public Button retryButton;
    public Button mainMenuButton;

    [Header("�� �̸� ����")]
    public string gameStageSceneName = "GameStage"; // ���� ���� �������� �� �̸�
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
            Debug.LogError("GameOverManager: RetryButton�� ������� �ʾҽ��ϴ�!");
        }

        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.AddListener(GoToMainMenu);
        }
        else
        {
            Debug.LogError("GameOverManager: MainMenuButton�� ������� �ʾҽ��ϴ�!");
        }

        // ���� ���� ȭ�鿡���� ���콺 Ŀ���� ���̵��� ����
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void RetryGame()
    {
        Debug.Log($"GameOverManager: '{gameStageSceneName}' ������ ���� ��õ�.");
        if (GameRoot.Instance != null)
        {
            // ������ ó������ (�������� 0����) �ٽ� �����մϴ�.
            GameRoot.Instance.PrepareForNewGameSession();
        }
        SceneManager.LoadScene(gameStageSceneName);
    }

    public void GoToMainMenu()
    {
        Debug.Log($"GameOverManager: '{mainMenuSceneName}' ������ �̵�.");
        // ���� �޴����� "�� ���� ����" �� GameRoot ���°� �ʱ�ȭ�ǹǷ�,
        // ���⼭�� ������ GameRoot �ʱ�ȭ ȣ���� �ʿ� ���� �� �ֽ��ϴ�.
        SceneManager.LoadScene(mainMenuSceneName);
    }

}
