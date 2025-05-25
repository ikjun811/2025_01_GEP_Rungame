using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class GameRoot : MonoBehaviour
{
    public static GameRoot Instance { get; private set; }

    public float step_timer = 0.0f;

    [Header("스테이지 관리")]
    private int _currentLogicalStageID = 0; // 데이터 파일의 STAGE_ID와 매칭 (0부터 시작)
    public int totalGameStages = 3;         // 실제 플레이 가능한 총 스테이지 수

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
            DontDestroyOnLoad(gameObject); // 단일 씬이라도 싱글톤 관리를 위해 유지 가능
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        if (stageClearUIPanel != null) stageClearUIPanel.SetActive(false);

        // 주요 컴포넌트 자동 찾기 (Inspector에서 할당하지 않았을 경우)
        if (levelControlInstance == null) levelControlInstance = FindObjectOfType<LevelControl>();
        if (mapCreatorInstance == null) mapCreatorInstance = FindObjectOfType<MapCreator>();
        if (playerControlInstance == null) playerControlInstance = FindObjectOfType<PlayerControl>();
        // bossInstance는 StartNewStage 또는 ResetBossForStage에서 찾거나 설정

        // 게임 시작 시 첫 번째 스테이지 설정
        _currentLogicalStageID = 0; // 항상 첫 번째 논리적 스테이지부터 시작
        StartNewStage(_currentLogicalStageID); // 첫 스테이지 시작
        UpdateStageDisplayText(); // 첫 스테이지 UI 표시
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

    public void NotifyStageCleared()
    {
        Debug.Log($"스테이지 (논리ID: {_currentLogicalStageID}) 클리어!");
        StartCoroutine(StageClearSequenceCoroutine());
    }

    private IEnumerator StageClearSequenceCoroutine()
    {
        if (stageClearUIPanel != null) stageClearUIPanel.SetActive(true);
        yield return new WaitForSecondsRealtime(stageClearDisplayTime);
        if (stageClearUIPanel != null) stageClearUIPanel.SetActive(false);

        _currentLogicalStageID++; // 다음 논리적 스테이지 ID로

        if (_currentLogicalStageID < totalGameStages)
        {
            Debug.Log($"다음 스테이지 시작 (논리 ID: {_currentLogicalStageID})");
            StartNewStage(_currentLogicalStageID);
        }
        else
        {
            Debug.Log("모든 스테이지를 클리어했습니다!");
            stageDisplayText.text = "ALL CLEAR!"; // 모든 스테이지 클리어 시 메시지 변경

        }
    }

    public void StartNewStage(int logicalStageID)
    {
        this._currentLogicalStageID = logicalStageID;

        if (levelControlInstance != null)
        {
            Debug.Log($"GameRoot: LevelControl 인스턴스 ({levelControlInstance.gameObject.name})의 ApplySettingsForStage 호출 시도 (ID: {logicalStageID})");
            levelControlInstance.ApplySettingsForStage(logicalStageID); // 에러 발생 라인
        }
        else
        {
            Debug.LogError("GameRoot: StartNewStage에서 LevelControl 인스턴스가 null입니다!");
        }

        if (mapCreatorInstance != null)
        {
            mapCreatorInstance.InitializeForNewStage();
        }
        else { Debug.LogError("GameRoot: MapCreator 인스턴스가 없습니다!"); }

        ResetBossForStage(logicalStageID);

        if (playerControlInstance != null)
        {
            // playerControlInstance.ResetForNewStage(); // 필요하다면 플레이어 상태 리셋
        }
        UpdateStageDisplayText(); // 새 스테이지 정보로 UI 업데이트
    }

    public void ResetBossForStage(int logicalStageID)
    {
        if (bossInstance == null) bossInstance = FindObjectOfType<BossControl>();

        if (bossInstance != null && levelControlInstance != null && levelControlInstance.currentStageSettings != null)
        {
            StageData currentStageData = levelControlInstance.currentStageSettings;
            if (currentStageData.bossMaxHp > 0)
            {
                bossInstance.gameObject.SetActive(true);
                // LevelControl의 ApplySettingsForStage에서 UpdateBossStats가 호출되므로,
                // 여기서는 보스의 다른 상태(위치, 공격 패턴 등)를 리셋하는 로직이 필요하다면 추가합니다.
                // bossInstance.ResetStateForNewStage(); // 예시: BossControl에 이런 함수를 만들 수 있음
                Debug.Log($"스테이지 {logicalStageID} 보스 준비 완료.");
            }
            else
            {
                bossInstance.gameObject.SetActive(false);
                Debug.Log($"스테이지 {logicalStageID}에는 보스가 없습니다.");
            }
        }
        // else Debug.LogWarning($"보스 리셋 불가: 스테이지 {logicalStageID}"); // 너무 많은 로그를 피하기 위해 주석 처리 가능
    }

    void UpdateStageDisplayText()
    {
        if (stageDisplayText != null)
        {
            if (_currentLogicalStageID >= 0 && _currentLogicalStageID < totalGameStages)
            {
                int displayStageNumber = _currentLogicalStageID + 1;
                stageDisplayText.text = $"STAGE {displayStageNumber}";
                stageDisplayText.gameObject.SetActive(true);
            }
            else if (_currentLogicalStageID >= totalGameStages) // 모든 스테이지 클리어 후
            {
                stageDisplayText.text = "ALL CLEAR!";
                stageDisplayText.gameObject.SetActive(true);
            }
            else // 그 외 (예: _currentLogicalStageID가 -1 등 유효하지 않은 값일 때)
            {
                stageDisplayText.text = "";
                // stageDisplayText.gameObject.SetActive(false);
            }
        }
    }

    public float getPlayTime()
    {
        return this.step_timer;
    }

    public int GetCurrentLogicalStageID()
    {
        return _currentLogicalStageID;
    }
}
