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
    public int currentStageID = 0;

    //private List<LevelData> level_datas = new List<LevelData>();
    public int HEIGHT_MAX = 20;
    public int HEIGHT_MIN = -4;

    public BossControl bossInstance; // 씬에 있는 보스 오브젝트를 Inspector에서 연결

    public bool generateSafeFloorOnly = false;


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
        LoadAllStageData(); // levelDataFile을 파싱하여 allStageData 리스트를 채움
        ApplyStageSettings(currentStageID);
        InitializeBlockGeneration(); // 블록 생성 관련 초기화
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

    public void ApplyStageSettings(int stageID)
    {
        currentStageSettings = allStageData.Find(s => s.stageID == stageID);
        if (currentStageSettings == null)
        {
            Debug.LogError($"스테이지 ID {stageID} 설정을 찾을 수 없습니다! 기본값을 사용하거나 첫 번째 스테이지를 사용합니다.");
            if (allStageData.Count > 0) currentStageSettings = allStageData[0]; // 임시: 첫 번째 스테이지 데이터 사용
            else currentStageSettings = new StageData(); // 임시 기본값
        }
        // 보스 스탯 업데이트
        if (bossInstance != null)
        {
            if (currentStageSettings.bossMaxHp > 0) // 보스가 등장하는 스테이지라면
            {
                bossInstance.UpdateBossStats(currentStageSettings.bossMaxHp, currentStageSettings.bossFireInterval);
            }
            else // 보스가 등장하지 않는 스테이지라면 비활성화
            {
                bossInstance.gameObject.SetActive(false);
                if (bossInstance.bossHealthSlider != null)
                {
                    bossInstance.bossHealthSlider.gameObject.SetActive(false);
                }
            }
        }
    }

    public float getPlayerSpeed()
    {
        return (currentStageSettings != null) ? currentStageSettings.playerSpeed : 10.0f; // 기본 속도
    }

    public void InitializeBlockGeneration()
    {
        this.block_count = 0;
        if (currentStageSettings != null) // currentStageSettings가 로드된 후에 호출되어야 함
        {
            // 안전 모드일 경우 첫 패턴을 강제로 긴 바닥으로 설정하거나,
            // PrepareNextBlockPatternForMapCreator가 처리하도록 current_count를 max_count로 설정
            current_block.block_type = Block.TYPE.HOLE; // 다음 PrepareNext... 호출 시 FLOOR가 되도록 유도
            current_block.max_count = 0; // 즉시 다음 패턴으로 넘어가도록
            current_block.height = 0;
            current_block.current_count = 0; // 또는 max_count로 설정하여 즉시 패턴 변경 유도

            // 첫 블록 패턴을 즉시 준비 (안전 모드 또는 일반 모드에 따라)
            PrepareNextBlockPatternForMapCreator();
            Debug.Log($"LevelControl: 블록 생성 초기화 및 첫 패턴 준비 완료. 현재 생성될 블록: {current_block.block_type}, 개수: {current_block.max_count}");
        }
    }


    public void PrepareNextBlockPatternForMapCreator() // MapCreator가 블록 묶음이 끝날 때 호출
    {
        if (currentStageSettings == null && !generateSafeFloorOnly)
        { // 안전 모드일 때는 currentStageSettings가 없어도 진행 가능
            Debug.LogError("PrepareNextBlockPatternForMapCreator: currentStageSettings가 null입니다!");
            return;
        }

        if (currentStageSettings == null) return;

        if (current_block.current_count >= current_block.max_count || block_count == 0) // 현재 블록 묶음 완료 또는 첫 시작
        {

            Block.TYPE nextType;
            int nextMaxCount;
            int nextHeight = current_block.height;


            if (generateSafeFloorOnly) // ⭐ 안전 모드이면 무조건 바닥 생성
            {
                nextType = Block.TYPE.FLOOR;
                nextMaxCount = 50; // 충분히 긴 안전한 바닥 (값은 조절 가능)
                nextHeight = 0;    // 평평한 바닥
                Debug.Log($"안전 바닥 생성 준비: 개수 {nextMaxCount}");
            }
            else // 일반 패턴 생성
            {
                if (current_block.block_type == Block.TYPE.FLOOR || block_count == 0)
                {
                    nextType = Block.TYPE.HOLE;
                    nextMaxCount = Random.Range(currentStageSettings.holeCount.min, currentStageSettings.holeCount.max + 1);
                }
                else
                {
                    nextType = Block.TYPE.FLOOR;
                    nextMaxCount = Random.Range(currentStageSettings.floorCount.min, currentStageSettings.floorCount.max + 1);
                    // 높이 변경 로직
                    int height_min = current_block.height + currentStageSettings.heightDiff.min;
                    int height_max = current_block.height + currentStageSettings.heightDiff.max;
                    height_min = Mathf.Clamp(height_min, HEIGHT_MIN, HEIGHT_MAX);
                    height_max = Mathf.Clamp(height_max, HEIGHT_MIN, HEIGHT_MAX);
                    nextHeight = Random.Range(height_min, height_max + 1);
                }
                // Debug.Log($"다음 패턴: {nextType}, 개수: {nextMaxCount}, 높이: {nextHeight}");
            }
            current_block.block_type = nextType;
            current_block.max_count = nextMaxCount;
            current_block.height = nextHeight;
            current_block.current_count = 0;
        }
        current_block.current_count++;
        block_count++;
    }

    public void SetSafeFloorMode(bool isSafe)
    {
        generateSafeFloorOnly = isSafe;
        Debug.Log($"LevelControl: SafeFloorMode 설정됨: {isSafe}");

        // 안전 모드가 켜지면, 즉시 다음 블록 패턴을 "안전한 바닥"으로 강제 설정 시도
        if (isSafe)
        {
            // 현재 블록 묶음을 강제로 완료시키고, 다음 PrepareNext... 호출 시 안전 바닥이 생성되도록 유도
            this.current_block.current_count = this.current_block.max_count;
            PrepareNextBlockPatternForMapCreator(); // 이 호출로 current_block이 안전한 바닥으로 설정됨
        }
        // 안전 모드가 해제되면, 다음 MapCreator의 PrepareNext... 호출 시 InitializeBlockGeneration에서
        // 설정된 일반 패턴으로 돌아감 (ApplySettingsForStage가 InitializeBlockGeneration을 호출했으므로)
    }

}