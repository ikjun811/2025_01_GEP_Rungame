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
    public string gameClearSceneName = "ClearMenu";

    [Header("UI 알림")]
    // Inspector에서 초기 할당 후, 씬 로드 시 다시 찾을 UI 요소들
    public GameObject stageClearUIPanel;
    public TextMeshProUGUI stageDisplayText; // 또는 public Text stageDisplayText;
    public float stageClearDisplayTime = 3.0f;

    // UI 오브젝트를 찾기 위한 이름 (Inspector에서 설정하거나 코드에 직접 명시)
    public string stageDisplayUiName = "StageNumText";
    public string stageClearPanelUiName = "StageClearText";

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
            SceneManager.sceneLoaded += OnGameSceneLoaded; 
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }

    }

    
    void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnGameSceneLoaded;
        }
    }

    void OnGameSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 로드된 씬이 실제 게임 플레이 씬인지 확인 (씬 이름으로 비교)
        if (scene.name == "GameStage") 
        {
            Debug.Log($"GameRoot: '{scene.name}' 로드됨. 현재 논리적 스테이지 ID: {_currentLogicalStageID} 기준으로 스테이지 설정 시작.");

            GameObject stageTextObj = GameObject.Find(stageDisplayUiName);
            if (stageTextObj != null)
            {
                stageDisplayText = stageTextObj.GetComponent<TextMeshProUGUI>(); // 사용하는 UI 타입에 맞게 변경 (Text 또는 TextMeshProUGUI)
                if (stageDisplayText == null)
                {
                    Debug.LogError($"GameRoot: '{stageDisplayUiName}' 오브젝트에서 TextMeshProUGUI (또는 Text) 컴포넌트를 찾지 못했습니다.");
                }
                else
                {
                    Debug.Log($"GameRoot: stageDisplayText ({stageDisplayUiName}) 참조 재설정 완료.");
                }
            }
            else
            {
                Debug.LogError($"GameRoot: '{stageDisplayUiName}' 이름을 가진 UI 오브젝트를 찾지 못했습니다. stageDisplayText 참조 실패.");
                stageDisplayText = null; // 못 찾았으면 null로 확실히 처리
            }

            GameObject stageClearPanelObj = GameObject.Find(stageClearPanelUiName);
            if (stageClearPanelObj != null)
            {
                stageClearUIPanel = stageClearPanelObj;
                Debug.Log($"GameRoot: stageClearUIPanel ({stageClearPanelUiName}) 참조 재설정 완료.");
                stageClearUIPanel.SetActive(false); // 찾은 후에는 일단 비활성화
            }
            else
            {
                Debug.LogError($"GameRoot: '{stageClearPanelUiName}' 이름을 가진 UI 오브젝트를 찾지 못했습니다. stageClearUIPanel 참조 실패.");
                stageClearUIPanel = null; // 못 찾았으면 null로 확실히 처리
            }


            levelControlInstance = FindObjectOfType<LevelControl>();
            mapCreatorInstance = FindObjectOfType<MapCreator>();
            playerControlInstance = FindObjectOfType<PlayerControl>();
            bossInstance = null;

            if (levelControlInstance == null) Debug.LogError("GameRoot OnGameSceneLoaded: LevelControl을 찾지 못했습니다!");
            if (mapCreatorInstance == null) Debug.LogError("GameRoot OnGameSceneLoaded: MapCreator를 찾지 못했습니다!");
            if (playerControlInstance == null) Debug.LogError("GameRoot OnGameSceneLoaded: PlayerControl을 찾지 못했습니다!");

            // 현재 _currentLogicalStageID에 맞춰 새 스테이지 환경 구성
            StartNewStage(_currentLogicalStageID);
            UpdateStageDisplayText();
        }
        else if(scene.name == "ClearMenu")
        {
            Debug.Log($"GameRoot: 게임 클리어 씬 '{scene.name}' 로드됨.");
            // 게임 클리어 씬에서는 GameRoot의 특정 UI 업데이트는 필요 없을 수 있습니다.
            // 예를 들어 stageDisplayText는 게임 스테이지용 UI일 수 있습니다.
            if (stageDisplayText != null) stageDisplayText.gameObject.SetActive(false);
        }
        else
        {
          Debug.Log($"GameRoot: 게임 플레이/클리어 씬이 아닌 '{scene.name}' 로드됨.");
            if (stageDisplayText != null && stageDisplayText.gameObject != null) stageDisplayText.gameObject.SetActive(false);
            if (stageClearUIPanel != null && stageClearUIPanel.gameObject != null) stageClearUIPanel.SetActive(false);
        }
    }

    public void PrepareForNewGameSession()
    {
        _currentLogicalStageID = 0; // 첫 번째 논리적 스테이지(ID 0)로 리셋
        step_timer = 0.0f;        // 플레이 시간 리셋
        Debug.Log("GameRoot: 새 게임 세션을 위해 상태 초기화됨. 논리 스테이지 ID: 0");
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
        return this.step_timer; 
    }

    private IEnumerator StageClearSequenceCoroutine()
    {
        if (stageClearUIPanel != null)
        {
            Debug.Log("GameRoot.StageClearSequenceCoroutine: stageClearUIPanel 활성화 시도.");
            stageClearUIPanel.SetActive(true);
        }
        else
        {
            Debug.LogError("GameRoot.StageClearSequenceCoroutine: stageClearUIPanel 참조가 NULL이라 활성화할 수 없습니다!");
        }

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


        if (stageClearUIPanel != null)
        {
            stageClearUIPanel.SetActive(false);
        }

        // 4. 다음 스테이지로 논리적 ID 업데이트
        _currentLogicalStageID++;

        if (_currentLogicalStageID < totalGameStages)
        {
            Debug.Log($"다음 스테이지 시작 준비 (논리 ID: {_currentLogicalStageID})");
            StartNewStage(_currentLogicalStageID);
        }
        else
        {
            Debug.Log("모든 스테이지를 클리어했습니다! 게임 클리어 씬으로 이동합니다.");
            // stageDisplayText.text = "ALL CLEAR!"; // GameClearScene에서 자체 UI로 처리
            SceneManager.LoadScene("ClearMenu"); // 게임 클리어 씬 로드
        }
    }

    void StartNewStage(int logicalStageID)
    {
        this._currentLogicalStageID = logicalStageID;
        Debug.Log($"StartNewStage 호출됨 - 논리 ID: {logicalStageID}");

        // 1. LevelControl에 새 스테이지 설정 적용 (이때 안전 모드는 해제되어야 함)
        if (levelControlInstance != null)
        {
            levelControlInstance.SetSafeFloorMode(false);
            levelControlInstance.ApplyStageSettings(logicalStageID);
        }
        else { Debug.LogError("GameRoot: LevelControl 인스턴스가 없습니다!"); }

        // 2. MapCreator 상태 리셋 (LevelControl.ApplySettingsForStage 내부에서 InitializeBlockGeneration 호출)
        if (mapCreatorInstance != null && levelControlInstance != null)
        {
            mapCreatorInstance.InitializeForNewStage(this.levelControlInstance);
        }
        else
        {
            if (mapCreatorInstance == null) Debug.LogError("GameRoot: MapCreator 인스턴스가 없습니다!");
            if (levelControlInstance == null) Debug.LogError("GameRoot: MapCreator 초기화에 필요한 LevelControl 인스턴스가 없습니다!");
        }

        // 3. 보스 리셋 또는 새 보스 설정
        ResetBossForStage(logicalStageID);

        // 4. UI 업데이트
        UpdateStageDisplayText();
    }

    void ResetBossForStage(int logicalStageID)
    {
        Debug.Log($"[LOG] GameRoot.ResetBossForStage: 스테이지 {logicalStageID}. GameRoot의 levelControlInstance는 {(levelControlInstance == null ? "NULL" : "할당됨")}");
        if (bossInstance == null) bossInstance = FindObjectOfType<BossControl>();

        if (bossInstance != null && levelControlInstance != null && levelControlInstance.currentStageSettings != null)
        {
            StageData currentStageData = levelControlInstance.currentStageSettings;
            Debug.Log($"[LOG] GameRoot.ResetBossForStage: currentStageData.bossMaxHp = {currentStageData.bossMaxHp}"); // ⭐ 이 값 확인
            if (currentStageData.bossMaxHp > 0)
            {
                Debug.Log($"[LOG] GameRoot.ResetBossForStage: currentStageData.bossMaxHp > 0 이므로 ResetStateForNewStage 호출.");
                bossInstance.gameObject.SetActive(true);
                bossInstance.ResetStateForNewStage();
            }
            else
            {
                Debug.Log($"[LOG] GameRoot.ResetBossForStage: currentStageData.bossMaxHp <= 0 이므로 보스 비활성화.");
                
                bossInstance.gameObject.SetActive(false);
                if (bossInstance.bossHealthSlider != null)
                {
                    bossInstance.bossHealthSlider.gameObject.SetActive(false);
                }
            }
        }
        else
        {
            // 이 로그들은 어떤 참조가 없는지 더 명확하게 알려줄 수 있습니다.
            if (bossInstance == null) Debug.LogWarning("GameRoot: BossControl 인스턴스가 없습니다. 보스를 리셋할 수 없습니다.");
            if (levelControlInstance == null) Debug.LogWarning("GameRoot: LevelControl 인스턴스가 없습니다. 보스 설정을 알 수 없습니다.");
            else if (levelControlInstance.currentStageSettings == null) Debug.LogWarning("GameRoot: LevelControl에 currentStageSettings가 없습니다.");
        }
    }

    void UpdateStageDisplayText()
    {
        if (stageDisplayText != null && stageDisplayText.gameObject != null) // gameObject까지 확인
        {
            if (SceneManager.GetActiveScene().name == "GameStage")
            {
                if (_currentLogicalStageID >= 0 && _currentLogicalStageID < totalGameStages)
                {
                    int displayStageNumber = _currentLogicalStageID + 1;
                    stageDisplayText.text = $"STAGE {displayStageNumber}";
                    stageDisplayText.gameObject.SetActive(true);
                    Debug.Log($"UpdateStageDisplayText: 스테이지 UI 업데이트됨 - {stageDisplayText.text}");
                }
                else
                {
                    // ALL CLEAR는 GameClearScene에서 처리
                    stageDisplayText.gameObject.SetActive(false);
                }
            }
            else
            {
                stageDisplayText.gameObject.SetActive(false);
            }
        }
        else
        {
            Debug.LogWarning("UpdateStageDisplayText: stageDisplayText 참조가 NULL이거나 파괴되어 UI를 업데이트할 수 없습니다.");
        }
    }

    public int GetCurrentLogicalStageID() { return _currentLogicalStageID; }
}