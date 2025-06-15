using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [Header("����� �ҽ�")]
    [Tooltip("�������(BGM)�� ����� ����� �ҽ�")]
    public AudioSource bgmSource;
    [Tooltip("ȿ����(SFX)�� ����� ����� �ҽ�")]
    public AudioSource sfxSource;

    [Header("����� Ŭ��")]
    public AudioClip mainMenuBgm; // �Ǵ� ȿ������ ��� one-shot���� ó��
    public AudioClip gameStageBgm;
    public AudioClip gameClearSfx;
    public AudioClip gameOverSfx;


    [Header("�÷��̾� ȿ����")] // ī�װ��� ������ָ� Inspector���� ���� �����ϴ�.
    public AudioClip playerDashSfx;
    public AudioClip playerHitSfx;

    [Header("������ ȿ����")]
    public AudioClip stoneAcquireSfx;
    public AudioClip stoneThrowSfx;
    public AudioClip bossHitSfx;
    public AudioClip shieldBreakSfx;
    public AudioClip healthRestoreSfx;
    public AudioClip shieldActivateSfx;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // ���� �ٲ� �ı����� ����

            // ���� �ε�� ������ ȣ��� �Լ��� ���
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void OnDestroy()
    {
        // ������Ʈ �ı� �� ����ߴ� �Լ��� ����
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // ���� �ε�� ������ ȣ��Ǵ� �Լ�
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // ���� ���� ��� ���� BGM�� ����
        bgmSource.Stop();
        sfxSource.Stop();

        // �� �̸��� ���� ������ BGM �Ǵ� ȿ���� ���
        if (scene.name == "MainMenu") // (�� �̸��� ���� ������Ʈ�� �°� Ȯ���ϼ���)
        {
            // ���� �޴��� BGM�̹Ƿ� Play ���, ȿ�����̶�� PlayOneShot ���
            bgmSource.clip = mainMenuBgm;
            bgmSource.Play();
        }
        else if (scene.name == "GameStage") // (�� �̸��� ���� ������Ʈ�� �°� Ȯ���ϼ���)
        {
            bgmSource.clip = gameStageBgm;
            bgmSource.Play(); // ��� �ݺ� ���
        }
        else if (scene.name == "ClearMenu") // (�� �̸��� ���� ������Ʈ�� �°� Ȯ���ϼ���)
        {
            sfxSource.PlayOneShot(gameClearSfx); // �� ���� ���
        }
        else if (scene.name == "GameOver") // (�� �̸��� ���� ������Ʈ�� �°� Ȯ���ϼ���)
        {
            sfxSource.PlayOneShot(gameOverSfx); // �� ���� ���
        }
    }

    // �ܺο��� ȿ������ ����� �� ȣ���� ���� �Լ�
    public void PlaySfx(AudioClip clip)
    {
        if (clip != null)
        {
            sfxSource.PlayOneShot(clip);
        }
    }
}
