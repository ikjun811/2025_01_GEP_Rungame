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
                bossInstance.gameObject.SetActive(true);
                bossInstance.UpdateBossStats(currentStageSettings.bossMaxHp, currentStageSettings.bossFireInterval);
            }
            else // 보스가 등장하지 않는 스테이지라면 비활성화
            {
                bossInstance.gameObject.SetActive(false);
            }
        }

        // PlayerControl, MapCreator, BossControl에 변경된 설정 알리기 또는 이들이 직접 가져가도록 함
        // 예: FindObjectOfType<PlayerControl>()?.SetSpeed(currentStageSettings.playerSpeed);
        //    FindObjectOfType<BossControl>()?.UpdateBossStats(currentStageSettings.bossMaxHp, currentStageSettings.bossFireInterval);
        //    FindObjectOfType<MapCreator>()?.UpdateMapRules(currentStageSettings.floorCount, ...);
        // 또는 각 스크립트가 Start()나 필요시 LevelControl.currentStageSettings를 직접 참조
    }

    public float getPlayerSpeed()
    {
        return (currentStageSettings != null) ? currentStageSettings.playerSpeed : 10.0f; // 기본 속도
    }

    void InitializeBlockGeneration()
    {
        this.block_count = 0;
        // current_block 초기화 (예: 첫 블록은 항상 바닥)
        current_block.block_type = Block.TYPE.FLOOR;
        current_block.max_count = (currentStageSettings != null) ? Random.Range(currentStageSettings.floorCount.min, currentStageSettings.floorCount.max + 1) : 10; // 첫 바닥 길이
        current_block.height = 0; // 시작 높이
        current_block.current_count = 0; // 아직 하나도 안 만듦 (PrepareNextBlockPatternForMapCreator에서 증가시킬 것이므로)
    }


    public void PrepareNextBlockPatternForMapCreator() // MapCreator가 블록 묶음이 끝날 때 호출
    {
        if (currentStageSettings == null) return;

        // 현재 current_block 묶음(예: FLOOR 5개)이 끝났는지 판단 (MapCreator가 알려주거나, 여기서 카운트)
        // 끝났다면, 다음 묶음(HOLE 또는 FLOOR)을 currentStageSettings의 규칙에 따라 결정
        // 예: 이전이 FLOOR였다면 다음은 HOLE, 개수는 holeCount.min/max 사이 랜덤
        //     이전이 HOLE이었다면 다음은 FLOOR, 개수는 floorCount.min/max 사이 랜덤, 높이도 heightDiff 범위 내 변경

        // 아래는 기존 update_level과 유사한 로직을 현재 스테이지 데이터 기반으로 수정하는 예시
        // (이 로직은 MapCreator가 current_block을 어떻게 소비하는지에 따라 달라짐)
        if (current_block.current_count >= current_block.max_count || block_count == 0) // 현재 블록 묶음 완료 또는 첫 시작
        {
            // previous_block = current_block; // 필요하다면
            // current_block = next_block;    // 이런식으로 3단계를 쓰거나, 바로 다음것 계산

            Block.TYPE nextType;
            int nextMaxCount;
            int nextHeight = current_block.height;


            if (current_block.block_type == Block.TYPE.FLOOR || block_count == 0)
            {
                nextType = Block.TYPE.HOLE;
                nextMaxCount = Random.Range(currentStageSettings.holeCount.min, currentStageSettings.holeCount.max + 1);
                Debug.Log($"다음 패턴: HOLE, 개수(min-max): {currentStageSettings.holeCount.min}-{currentStageSettings.holeCount.max}, 선택된 개수: {nextMaxCount}");
            }
            else // 이전이 구멍이면 바닥
            {
                nextType = Block.TYPE.FLOOR;
                nextMaxCount = Random.Range(currentStageSettings.floorCount.min, currentStageSettings.floorCount.max + 1);
                int height_min = current_block.height + currentStageSettings.heightDiff.min;
                int height_max = current_block.height + currentStageSettings.heightDiff.max;
                height_min = Mathf.Clamp(height_min, HEIGHT_MIN, HEIGHT_MAX);
                height_max = Mathf.Clamp(height_max, HEIGHT_MIN, HEIGHT_MAX);
                nextHeight = Random.Range(height_min, height_max + 1);
                Debug.Log($"다음 패턴: FLOOR, 개수(min-max): {currentStageSettings.floorCount.min}-{currentStageSettings.floorCount.max}, 선택된 개수: {nextMaxCount}, 높이: {nextHeight}");
            }

            current_block.block_type = nextType;
            current_block.max_count = nextMaxCount;
            current_block.height = nextHeight;
            current_block.current_count = 0;
        }
        current_block.current_count++;
        block_count++;
    }


}