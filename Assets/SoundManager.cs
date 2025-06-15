using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [Header("오디오 소스")]
    [Tooltip("배경음악(BGM)을 재생할 오디오 소스")]
    public AudioSource bgmSource;
    [Tooltip("효과음(SFX)을 재생할 오디오 소스")]
    public AudioSource sfxSource;

    [Header("오디오 클립")]
    public AudioClip mainMenuBgm; // 또는 효과음일 경우 one-shot으로 처리
    public AudioClip gameStageBgm;
    public AudioClip gameClearSfx;
    public AudioClip gameOverSfx;


    [Header("플레이어 효과음")] // 카테고리를 만들어주면 Inspector에서 보기 좋습니다.
    public AudioClip playerDashSfx;
    public AudioClip playerHitSfx;

    [Header("아이템 효과음")]
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
            DontDestroyOnLoad(gameObject); // 씬이 바뀌어도 파괴되지 않음

            // 씬이 로드될 때마다 호출될 함수를 등록
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void OnDestroy()
    {
        // 오브젝트 파괴 시 등록했던 함수를 해제
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // 씬이 로드될 때마다 호출되는 함수
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 먼저 현재 재생 중인 BGM을 멈춤
        bgmSource.Stop();
        sfxSource.Stop();

        // 씬 이름에 따라 적절한 BGM 또는 효과음 재생
        if (scene.name == "MainMenu") // (씬 이름을 실제 프로젝트에 맞게 확인하세요)
        {
            // 메인 메뉴는 BGM이므로 Play 사용, 효과음이라면 PlayOneShot 사용
            bgmSource.clip = mainMenuBgm;
            bgmSource.Play();
        }
        else if (scene.name == "GameStage") // (씬 이름을 실제 프로젝트에 맞게 확인하세요)
        {
            bgmSource.clip = gameStageBgm;
            bgmSource.Play(); // 계속 반복 재생
        }
        else if (scene.name == "ClearMenu") // (씬 이름을 실제 프로젝트에 맞게 확인하세요)
        {
            sfxSource.PlayOneShot(gameClearSfx); // 한 번만 재생
        }
        else if (scene.name == "GameOver") // (씬 이름을 실제 프로젝트에 맞게 확인하세요)
        {
            sfxSource.PlayOneShot(gameOverSfx); // 한 번만 재생
        }
    }

    // 외부에서 효과음을 재생할 때 호출할 공용 함수
    public void PlaySfx(AudioClip clip)
    {
        if (clip != null)
        {
            sfxSource.PlayOneShot(clip);
        }
    }
}
