using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;




[System.Serializable]
public class StageData
{
    public int stageID; 
    public float playerSpeed;
    public LevelControl.Range floorCount; 
    public LevelControl.Range holeCount;
    public LevelControl.Range heightDiff;
    public float bossMaxHp;
    public float bossFireInterval;
}


public class LevelControl : MonoBehaviour
{
    public TextAsset levelDataFile; // 기존 level_data_text 대신 이 이름 사용 또는 유지
    public List<StageData> allStageData = new List<StageData>();
    public StageData currentStageSettings; // 현재 스테이지의 설정값
    public int currentStageID = -1;

    //private List<LevelData> level_datas = new List<LevelData>();
    public int HEIGHT_MAX = 20;
    public int HEIGHT_MIN = -4;

    public BossControl bossInstance; // 씬에 있는 보스 오브젝트를 Inspector에서 연결


    [System.Serializable]
    public struct Range
    {
        public int min;
        public int max;
    };

    public struct CreationInfo
    {
        public Block.TYPE block_type; // 블록의 종류
        public int max_count; // 블록의 최대 개수
        public int height; // 블록을 배치할 높이
        public int current_count; // 작성한 블록의 개수
    };
    //public CreationInfo previous_block; // 이전에 어떤 블록을 만들었는가
    public CreationInfo current_block; // 지금 어떤 블록을 만들어야 하는가
    //public CreationInfo next_block; // 다음에 어떤 블록을 만들어야 하는가

    public int block_count = 0; // 생성한 블록의 총 수



    void Awake() // 또는 Start()
    {
        LoadAllStageData();
    }

    void LoadAllStageData()
    {
        allStageData.Clear();
        if (levelDataFile == null)
        {
            Debug.LogError("LevelDataFile이 할당되지 않았습니다!");
            return;
        }

        string[] lines = levelDataFile.text.Split('\n');
        foreach (string line in lines)
        {
            if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#")) continue;

            string[] values = line.Split(' '); // 탭으로 구분했었다면 \t, 공백이면 ' '
            if (values.Length < 10) // STAGE_ID부터 BOSS_FIRE_INTERVAL까지 최소 10개 값
            {
                Debug.LogWarning($"잘못된 데이터 라인: {line}");
                continue;
            }
            try
            {
                StageData stage = new StageData();
                stage.stageID = int.Parse(values[0]);
                stage.playerSpeed = float.Parse(values[1]);
                stage.floorCount.min = int.Parse(values[2]);
                stage.floorCount.max = int.Parse(values[3]);
                stage.holeCount.min = int.Parse(values[4]);
                stage.holeCount.max = int.Parse(values[5]);
                stage.heightDiff.min = int.Parse(values[6]);
                stage.heightDiff.max = int.Parse(values[7]);
                stage.bossMaxHp = float.Parse(values[8]);
                stage.bossFireInterval = float.Parse(values[9]);
                allStageData.Add(stage);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"데이터 파싱 오류: {line} - {e.Message}");
            }
        }
    }

    public void ApplySettingsForStage(int logicalStageID)
    {
        this.currentStageID = logicalStageID;
        Debug.Log($"LevelControl: 스테이지 ID {currentStageID}에 대한 설정 적용 시작.");

        currentStageSettings = allStageData.Find(s => s.stageID == this.currentStageID);

        if (currentStageSettings == null)
        {
            Debug.LogError($"스테이지 ID {this.currentStageID}에 해당하는 설정을 찾을 수 없습니다! level_data.txt와 GameRoot의 스테이지 ID 계산 로직을 확인해주세요.");
            if (allStageData.Count > 0) currentStageSettings = allStageData[0];
            else currentStageSettings = new StageData();
        }

        // Debug.Log($"적용된 스테이지 설정: PlayerSpeed={currentStageSettings.playerSpeed}, BossHP={currentStageSettings.bossMaxHp}, FloorMin={currentStageSettings.floorCount.min}"); // 상세 로그

        if (bossInstance != null)
        {
            if (currentStageSettings.bossMaxHp > 0)
            {
                bossInstance.gameObject.SetActive(true);
                bossInstance.UpdateBossStats(currentStageSettings.bossMaxHp, currentStageSettings.bossFireInterval);
            }
            else
            {
                bossInstance.gameObject.SetActive(false);
            }
        }

        InitializeBlockGeneration();
    }

    public float getPlayerSpeed()
    {
        return (currentStageSettings != null) ? currentStageSettings.playerSpeed : 10.0f; // 기본 속도
    }

    public void InitializeBlockGeneration()
    {
        this.block_count = 0; // 총 블록 카운트 초기화
        if (currentStageSettings != null)
        {
            // 첫 번째 블록 묶음은 항상 FLOOR로 설정
            current_block.block_type = Block.TYPE.FLOOR;
            current_block.max_count = Random.Range(Mathf.Max(1, currentStageSettings.floorCount.min), currentStageSettings.floorCount.max + 1);
            current_block.height = 0; // 시작 높이
            current_block.current_count = 0; 
            Debug.Log($"LevelControl: 블록 생성 초기화 완료. 첫 묶음: {current_block.block_type}, 개수: {current_block.max_count}");
        }
        else
        {
            Debug.LogError("InitializeBlockGeneration: currentStageSettings가 null입니다.");
            // 기본값으로라도 current_block을 설정해주는 것이 안전할 수 있습니다.
            current_block.block_type = Block.TYPE.FLOOR;
            current_block.max_count = 10;
            current_block.height = 0;
            current_block.current_count = 0;
        }
    }


    public void PrepareNextBlockPatternForMapCreator() // MapCreator가 블록 묶음이 끝날 때 호출
    {
        if (currentStageSettings == null)
        {
            Debug.LogError("PrepareNextBlockPatternForMapCreator: currentStageSettings가 null입니다!");
            return; // currentStageSettings가 없으면 패턴을 결정할 수 없음
        }

        // 현재 블록 묶음의 생성이 완료되었을 때만 다음 패턴으로 변경
        // block_count > 0 조건을 추가하여 게임 시작 후 첫 번째 호출에서는 패턴 변경 시도를 막고,
        // InitializeBlockGeneration에서 설정한 첫 번째 묶음을 사용하도록 합니다.
        if (current_block.current_count >= current_block.max_count && block_count > 0)
        {
            // Debug.Log($"===== 새 블록 패턴 결정 시작 (이전 타입: {current_block.block_type}, 이전 카운트: {current_block.current_count}/{current_block.max_count}) =====");
            // ... (기존의 nextType, nextMaxCount, nextHeight 결정 로직은 동일하게 사용) ...
            Block.TYPE nextType;
            int nextMaxCount;
            int nextHeight = current_block.height;

            if (current_block.block_type == Block.TYPE.FLOOR) // 이전이 바닥이면 다음은 구멍
            {
                nextType = Block.TYPE.HOLE;
                nextMaxCount = Random.Range(currentStageSettings.holeCount.min, currentStageSettings.holeCount.max + 1);
                // Debug.Log($"다음 패턴: HOLE, 선택된 개수: {nextMaxCount}");
            }
            else // 이전이 구멍이면 다음은 바닥
            {
                nextType = Block.TYPE.FLOOR;
                nextMaxCount = Random.Range(currentStageSettings.floorCount.min, currentStageSettings.floorCount.max + 1);
                // (높이 변경 로직)
                int height_min = current_block.height + currentStageSettings.heightDiff.min;
                int height_max = current_block.height + currentStageSettings.heightDiff.max;
                height_min = Mathf.Clamp(height_min, HEIGHT_MIN, HEIGHT_MAX);
                height_max = Mathf.Clamp(height_max, HEIGHT_MIN, HEIGHT_MAX);
                nextHeight = Random.Range(height_min, height_max + 1);
                // Debug.Log($"다음 패턴: FLOOR, 선택된 개수: {nextMaxCount}, 높이: {nextHeight}");
            }

            current_block.block_type = nextType;
            current_block.max_count = nextMaxCount;
            current_block.height = nextHeight;
            current_block.current_count = 0; // 새 패턴 시작, 카운트 리셋
        }
        current_block.current_count++; // 이번에 MapCreator가 사용할 블록을 위해 카운트 증가
        block_count++; // 전체 블록 카운트 증가


    }
}
