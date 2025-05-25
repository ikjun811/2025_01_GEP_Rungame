using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameRoot : MonoBehaviour
{
    public static GameRoot Instance { get; private set; }

    public float step_timer = 0.0f; // 경과 시간을 유지

    [Header("스테이지 관리")]
    private int _currentLogicalStageID = 0;
    public int totalGameStages = 3;

    [Header("UI 알림")]
    public GameObject stageClearUIPanel;
    public float stageClearDisplayTime = 3.0f;
    public TextMeshProUGUI stageDisplayText;

    [Header("주요 게임 컴포넌트 참조")]
    public LevelControl levelControlInstance;
    public MapCreator mapCreatorInstance;
    public PlayerControl playerControlInstance;
    public BossControl bossInstance;


    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        if (stageClearUIPanel != null) stageClearUIPanel.SetActive(false);

        if (levelControlInstance == null) levelControlInstance = FindObjectOfType<LevelControl>();
        if (mapCreatorInstance == null) mapCreatorInstance = FindObjectOfType<MapCreator>();
        if (playerControlInstance == null) playerControlInstance = FindObjectOfType<PlayerControl>();

        _currentLogicalStageID = 0; // 첫 스테이지 ID
        StartNewStage(_currentLogicalStageID, true); // true: 게임 첫 시작 시
        UpdateStageDisplayText();
    }

    public void NotifyStageCleared()
    {
        Debug.Log($"스테이지 (논리ID: {_currentLogicalStageID}) 클리어!");
        StartCoroutine(StageClearSequenceCoroutine());
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        this.step_timer += Time.deltaTime; // 경과 시간을 더함
    }

    public float getPlayTime()
    {
        float time;
        time = this.step_timer;
        return (time); // 호출한 곳에 경과 시간을 알려줌
    }

    private IEnumerator StageClearSequenceCoroutine()
    {
        if (stageClearUIPanel != null) stageClearUIPanel.SetActive(true);

        // 1. 플레이어 체력 회복
        if (playerControlInstance != null)
        {
            playerControlInstance.RestoreHealth();
            Debug.Log("플레이어 체력 회복됨.");
        }

        // 2. LevelControl에 "안전한 바닥" 생성 모드 요청
        if (levelControlInstance != null)
        {
            levelControlInstance.SetSafeFloorMode(true); // 안전 모드 시작
        }

        // 3. 알림 메시지 표시 시간 동안 대기 (이 시간 동안 안전한 바닥 생성)
        yield return new WaitForSecondsRealtime(stageClearDisplayTime);

        if (stageClearUIPanel != null) stageClearUIPanel.SetActive(false);

        // 4. 다음 스테이지로 논리적 ID 업데이트
        _currentLogicalStageID++;

        if (_currentLogicalStageID < totalGameStages)
        {
            Debug.Log($"다음 스테이지 시작 준비 (논리 ID: {_currentLogicalStageID})");
            StartNewStage(_currentLogicalStageID, false); // 새 스테이지 시작 (첫 시작 아님)
        }
        else
        {
            Debug.Log("모든 스테이지를 클리어했습니다!");
            stageDisplayText.text = "ALL CLEAR!";
            // 추가적인 게임 클리어 처리 (예: 게임 종료, 크레딧 씬 로드 등)
        }
    }

    void StartNewStage(int logicalStageID, bool isFirstLaunch)
    {
        this._currentLogicalStageID = logicalStageID;
        Debug.Log($"StartNewStage 호출됨 - 논리 ID: {logicalStageID}, 첫 실행: {isFirstLaunch}");

        // 1. LevelControl에 새 스테이지 설정 적용 (이때 안전 모드는 해제되어야 함)
        if (levelControlInstance != null)
        {
            if (!isFirstLaunch) // 게임 첫 실행이 아닐 때만 안전모드 해제 (첫 실행시는 기본적으로 안전모드 아님)
            {
                levelControlInstance.SetSafeFloorMode(false); // ⭐ 안전 모드 해제
            }
            levelControlInstance.ApplyStageSettings(logicalStageID);
        }
        else { Debug.LogError("GameRoot: LevelControl 인스턴스가 없습니다!"); }

        // 2. MapCreator 상태 리셋 (LevelControl.ApplySettingsForStage 내부에서 InitializeBlockGeneration 호출)
        if (mapCreatorInstance != null)
        {
            mapCreatorInstance.InitializeForNewStage(this.levelControlInstance);
        }
        else { Debug.LogError("GameRoot: MapCreator 인스턴스가 없습니다!"); }

        // 3. 보스 리셋 또는 새 보스 설정
        ResetBossForStage(logicalStageID);

        // 4. UI 업데이트
        UpdateStageDisplayText();
    }

    void ResetBossForStage(int logicalStageID)
    {
        if (bossInstance == null) bossInstance = FindObjectOfType<BossControl>();

        if (bossInstance != null && levelControlInstance != null && levelControlInstance.currentStageSettings != null)
        {
            StageData currentStageData = levelControlInstance.currentStageSettings;
            if (currentStageData.bossMaxHp > 0) // 현재 스테이지에 보스가 있다면
            {
                bossInstance.ResetStateForNewStage(); 
                Debug.Log($"스테이지 {logicalStageID} 보스 준비 완료 (ResetStateForNewStage 호출됨).");
            }
            else // 현재 스테이지에 보스가 없다면
            {
                bossInstance.gameObject.SetActive(false);
                if (bossInstance.bossHealthSlider != null) // 보스가 비활성화되므로 슬라이더도 숨김
                {
                    bossInstance.bossHealthSlider.gameObject.SetActive(false);
                }
                Debug.Log($"스테이지 {logicalStageID}에는 보스가 없습니다.");
            }
        }
    }

    void UpdateStageDisplayText()
    {
        // ... (이전 답변의 UpdateStageDisplayText 로직과 동일) ...
        if (stageDisplayText != null)
        {
            if (_currentLogicalStageID >= 0 && _currentLogicalStageID < totalGameStages)
            {
                int displayStageNumber = _currentLogicalStageID + 1;
                stageDisplayText.text = $"STAGE {displayStageNumber}";
                stageDisplayText.gameObject.SetActive(true);
            }
            else if (_currentLogicalStageID >= totalGameStages)
            {
                stageDisplayText.text = "ALL CLEAR!";
                stageDisplayText.gameObject.SetActive(true);
            }
            else
            {
                stageDisplayText.text = "";
            }
        }
    }

    public int GetCurrentLogicalStageID() { return _currentLogicalStageID; }
}