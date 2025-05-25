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
    public TextAsset levelDataFile; // ���� level_data_text ��� �� �̸� ��� �Ǵ� ����
    public List<StageData> allStageData = new List<StageData>();
    public StageData currentStageSettings; // ���� ���������� ������
    public int currentStageID = 0;

    //private List<LevelData> level_datas = new List<LevelData>();
    public int HEIGHT_MAX = 20;
    public int HEIGHT_MIN = -4;

    public BossControl bossInstance; // ���� �ִ� ���� ������Ʈ�� Inspector���� ����


    [System.Serializable]
    public struct Range
    {
        public int min;
        public int max;
    };

    public struct CreationInfo
    {
        public Block.TYPE block_type; // ����� ����
        public int max_count; // ����� �ִ� ����
        public int height; // ����� ��ġ�� ����
        public int current_count; // �ۼ��� ����� ����
    };
    //public CreationInfo previous_block; // ������ � ����� ������°�
    public CreationInfo current_block; // ���� � ����� ������ �ϴ°�
    //public CreationInfo next_block; // ������ � ����� ������ �ϴ°�

    public int block_count = 0; // ������ ����� �� ��



    void Awake() // �Ǵ� Start()
    {
        LoadAllStageData(); // levelDataFile�� �Ľ��Ͽ� allStageData ����Ʈ�� ä��
        ApplyStageSettings(currentStageID);
        InitializeBlockGeneration(); // ��� ���� ���� �ʱ�ȭ
    }

    void LoadAllStageData()
    {
        allStageData.Clear();
        if (levelDataFile == null)
        {
            Debug.LogError("LevelDataFile�� �Ҵ���� �ʾҽ��ϴ�!");
            return;
        }

        string[] lines = levelDataFile.text.Split('\n');
        foreach (string line in lines)
        {
            if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#")) continue;

            string[] values = line.Split(' '); // ������ �����߾��ٸ� \t, �����̸� ' '
            if (values.Length < 10) // STAGE_ID���� BOSS_FIRE_INTERVAL���� �ּ� 10�� ��
            {
                Debug.LogWarning($"�߸��� ������ ����: {line}");
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
                Debug.LogError($"������ �Ľ� ����: {line} - {e.Message}");
            }
        }
    }

    public void ApplyStageSettings(int stageID)
    {
        currentStageSettings = allStageData.Find(s => s.stageID == stageID);
        if (currentStageSettings == null)
        {
            Debug.LogError($"�������� ID {stageID} ������ ã�� �� �����ϴ�! �⺻���� ����ϰų� ù ��° ���������� ����մϴ�.");
            if (allStageData.Count > 0) currentStageSettings = allStageData[0]; // �ӽ�: ù ��° �������� ������ ���
            else currentStageSettings = new StageData(); // �ӽ� �⺻��
        }
        // ���� ���� ������Ʈ
        if (bossInstance != null)
        {
            if (currentStageSettings.bossMaxHp > 0) // ������ �����ϴ� �����������
            {
                bossInstance.gameObject.SetActive(true);
                bossInstance.UpdateBossStats(currentStageSettings.bossMaxHp, currentStageSettings.bossFireInterval);
            }
            else // ������ �������� �ʴ� ����������� ��Ȱ��ȭ
            {
                bossInstance.gameObject.SetActive(false);
            }
        }

        // PlayerControl, MapCreator, BossControl�� ����� ���� �˸��� �Ǵ� �̵��� ���� ���������� ��
        // ��: FindObjectOfType<PlayerControl>()?.SetSpeed(currentStageSettings.playerSpeed);
        //    FindObjectOfType<BossControl>()?.UpdateBossStats(currentStageSettings.bossMaxHp, currentStageSettings.bossFireInterval);
        //    FindObjectOfType<MapCreator>()?.UpdateMapRules(currentStageSettings.floorCount, ...);
        // �Ǵ� �� ��ũ��Ʈ�� Start()�� �ʿ�� LevelControl.currentStageSettings�� ���� ����
    }

    public float getPlayerSpeed()
    {
        return (currentStageSettings != null) ? currentStageSettings.playerSpeed : 10.0f; // �⺻ �ӵ�
    }

    void InitializeBlockGeneration()
    {
        this.block_count = 0;
        // current_block �ʱ�ȭ (��: ù ����� �׻� �ٴ�)
        current_block.block_type = Block.TYPE.FLOOR;
        current_block.max_count = (currentStageSettings != null) ? Random.Range(currentStageSettings.floorCount.min, currentStageSettings.floorCount.max + 1) : 10; // ù �ٴ� ����
        current_block.height = 0; // ���� ����
        current_block.current_count = 0; // ���� �ϳ��� �� ���� (PrepareNextBlockPatternForMapCreator���� ������ų ���̹Ƿ�)
    }


    public void PrepareNextBlockPatternForMapCreator() // MapCreator�� ��� ������ ���� �� ȣ��
    {
        if (currentStageSettings == null) return;

        // ���� current_block ����(��: FLOOR 5��)�� �������� �Ǵ� (MapCreator�� �˷��ְų�, ���⼭ ī��Ʈ)
        // �����ٸ�, ���� ����(HOLE �Ǵ� FLOOR)�� currentStageSettings�� ��Ģ�� ���� ����
        // ��: ������ FLOOR���ٸ� ������ HOLE, ������ holeCount.min/max ���� ����
        //     ������ HOLE�̾��ٸ� ������ FLOOR, ������ floorCount.min/max ���� ����, ���̵� heightDiff ���� �� ����

        // �Ʒ��� ���� update_level�� ������ ������ ���� �������� ������ ������� �����ϴ� ����
        // (�� ������ MapCreator�� current_block�� ��� �Һ��ϴ����� ���� �޶���)
        if (current_block.current_count >= current_block.max_count || block_count == 0) // ���� ��� ���� �Ϸ� �Ǵ� ù ����
        {
            // previous_block = current_block; // �ʿ��ϴٸ�
            // current_block = next_block;    // �̷������� 3�ܰ踦 ���ų�, �ٷ� ������ ���

            Block.TYPE nextType;
            int nextMaxCount;
            int nextHeight = current_block.height;


            if (current_block.block_type == Block.TYPE.FLOOR || block_count == 0)
            {
                nextType = Block.TYPE.HOLE;
                nextMaxCount = Random.Range(currentStageSettings.holeCount.min, currentStageSettings.holeCount.max + 1);
                Debug.Log($"���� ����: HOLE, ����(min-max): {currentStageSettings.holeCount.min}-{currentStageSettings.holeCount.max}, ���õ� ����: {nextMaxCount}");
            }
            else // ������ �����̸� �ٴ�
            {
                nextType = Block.TYPE.FLOOR;
                nextMaxCount = Random.Range(currentStageSettings.floorCount.min, currentStageSettings.floorCount.max + 1);
                int height_min = current_block.height + currentStageSettings.heightDiff.min;
                int height_max = current_block.height + currentStageSettings.heightDiff.max;
                height_min = Mathf.Clamp(height_min, HEIGHT_MIN, HEIGHT_MAX);
                height_max = Mathf.Clamp(height_max, HEIGHT_MIN, HEIGHT_MAX);
                nextHeight = Random.Range(height_min, height_max + 1);
                Debug.Log($"���� ����: FLOOR, ����(min-max): {currentStageSettings.floorCount.min}-{currentStageSettings.floorCount.max}, ���õ� ����: {nextMaxCount}, ����: {nextHeight}");
            }

            current_block.block_type = nextType;
            current_block.max_count = nextMaxCount;
            current_block.height = nextHeight;
            current_block.current_count = 0;
        }
        current_block.current_count++;
        block_count++;
    }








    /*
    // ������ ��Ʈ�� ������ ���
    private void clear_next_block(ref CreationInfo block)
    {
        // ���޹��� ���(block)�� �ʱ�ȭ
        block.block_type = Block.TYPE.FLOOR;
        block.max_count = 15;
        block.height = 0;
        block.current_count = 0;
    }
    // ������ ��Ʈ�� �ʱ�ȭ
    public void initialize()
    {
        this.block_count = 0; // ����� �� ���� �ʱ�ȭ

        // clear_next_block()�� �Ѱܼ� �ʱ�ȭ

        this.clear_next_block(ref this.current_block);

    }*/

    /*
    private void update_level(ref CreationInfo current, CreationInfo previous)
    {
        switch (previous.block_type)
        {
            case Block.TYPE.FLOOR: // �̹� ����� �ٴ��� ���
                current.block_type = Block.TYPE.HOLE; // ���� ���� ������ ����
                current.max_count = 5; // ������ 5�� ����
                current.height = previous.height; // ���̸� ������ ����
                break;
            case Block.TYPE.HOLE: // �̹� ����� ������ ���
                current.block_type = Block.TYPE.FLOOR; // ������ �ٴ� ����
                current.max_count = 10; // �ٴ��� 10�� ����
                break;
        }
    }*/


    /*
    public void update(float passage_time)
    { // *Update()�� �ƴ�, create_floor_block() �޼��忡�� ȣ��
        this.current_block.current_count++; // �̹��� ���� ��� ������ ����
                                            // �̹��� ���� ��� ������ max_count �̻��̸�,
        if (this.current_block.current_count >= this.current_block.max_count)
        {
            this.previous_block = this.current_block;
            this.current_block = this.next_block;
            this.clear_next_block(ref this.next_block); // ������ ���� ����� ������ �ʱ�ȭ
            //this.update_level(ref this.next_block, this.current_block); // ������ ���� ����� ����
            this.update_level(ref this.next_block, this.current_block, passage_time);
        }
        this.block_count++; // ����� �� ���� ����
    }*/

    /*
    public void loadLevelData(TextAsset level_data_text)
    {
        string level_texts = level_data_text.text; // �ؽ�Ʈ �����͸� ���ڿ��� ������
        string[] lines = level_texts.Split("\n"); // ���� �ڵ� ��\������ �����ؼ� ���ڿ� �迭�� ����
                                                  // lines ���� �� �࿡ ���ؼ� ���ʷ� ó���� ���� ����
        foreach (var line in lines)
        {
            if (line == "")
            { // ���� �� ���̸�,
                continue; // �Ʒ� ó���� ���� �ʰ� �ݺ����� ó������ ����
            };
            Debug.Log(line); // ���� ������ ����� ���
            string[] words = line.Split(); // �� ���� ���带 �迭�� ����
            int n = 0;
            // LevelData�� ������ ����
            // ���� ó���ϴ� ���� �����͸� ����
            LevelData level_data = new LevelData();
            // words���� �� ���忡 ���ؼ� ������� ó���� ���� ����
            foreach (var word in words)
            {
                if(word.StartsWith("#")) { // ������ ���۹��ڰ� #�̸�
                    break;
                } // ���� Ż��
                if (word == "")
                { // ���尡 �� �������
                    continue;
                } // ������ �������� ����
                  // n ���� 0, 1, 2,...7�� ��ȭ���� �����ν� 8�׸��� ó��
                  // �� ���带 �÷԰����� ��ȯ�ϰ� level_data�� ����
                switch (n)
                {
                    case 0: level_data.end_time = float.Parse(word); break;
                    case 1: level_data.player_speed = float.Parse(word); break;
                    case 2: level_data.floor_count.min = int.Parse(word); break;
                    case 3: level_data.floor_count.max = int.Parse(word); break;
                    case 4: level_data.hole_count.min = int.Parse(word); break;
                    case 5: level_data.hole_count.max = int.Parse(word); break;
                    case 6: level_data.height_diff.min = int.Parse(word); break;
                    case 7: level_data.height_diff.max = int.Parse(word); break;
                }
                n++;
            }
            if (n >= 8)
            { // 8�׸�(�̻�)�� ����� ó���Ǿ��ٸ�,
                this.level_datas.Add(level_data); // List ������ level_datas�� level_data�� �߰�
            }
            else
            { // �׷��� �ʴٸ�(������ ���ɼ��� ����),
                if (n == 0)
                { // 1���嵵 ó������ ���� ���� �ּ��̹Ƿ�
                  // �ƹ��͵� ���� ����
                }
                else
                { // �� �̿��̸� ������ ����
                  // ������ ������ ���� �ʴٴ� ���� �����ִ� ���� �޽����� ǥ��
                    Debug.LogError("[LevelData] Out of parameter.\n");
                }
            }
        }
        if (this.level_datas.Count == 0)
        { // level_datas�� �����Ͱ� �ϳ��� ������
            Debug.LogError("[LevelData] Has no data.\n"); // ���� �޽����� ǥ��
            this.level_datas.Add(new LevelData()); // level_datas�� �⺻ LevelData�� �ϳ� �߰�
        }
    }*/

    /*
    private void update_level(ref CreationInfo current, CreationInfo previous, float passage_time)
    { // �� �μ� passage_time���� �÷��� ��� �ð��� ����
      // ���� 1~���� 5�� �ݺ�
        float local_time = Mathf.Repeat(passage_time, this.level_datas[this.level_datas.Count - 1].end_time); // Mathf.Repeat: 0~max ������ ���� ��ȯ
                                                                                                              // ���� ������ ����
        int i;
        for (i = 0; i < this.level_datas.Count - 1; i++)
        {
            if (local_time <= this.level_datas[i].end_time)
            {
                break;
            }
        }
        this.level = i;
        current.block_type = Block.TYPE.FLOOR;
        current.max_count = 1;
        if (this.block_count >= 10)
        {
            // ���� ������ ���� �����͸� ������
            LevelData level_data;
            level_data = this.level_datas[this.level];
            switch (previous.block_type)
            {
                case Block.TYPE.FLOOR: // ���� ����� �ٴ��� ���,
                    current.block_type = Block.TYPE.HOLE; // �̹��� ������ ����
                                                          // ���� ũ���� �ּڰ�~�ִ� ������ ������ ��
                    current.max_count = Random.Range(level_data.hole_count.min, level_data.hole_count.max);
                    current.height = previous.height; // ���̸� ������ ����
                    break;
                case Block.TYPE.HOLE: // ���� ����� ������ ���,
                    current.block_type = Block.TYPE.FLOOR; // �̹��� �ٴ��� ����
                                                           // �ٴ� ������ �ּڰ�~�ִ� ������ ������ ��
                    current.max_count = Random.Range(level_data.floor_count.min, level_data.floor_count.max);
                    // �ٴ� ������ �ּڰ��� �ִ��� ����
                    int height_min = previous.height + level_data.height_diff.min;
                    int height_max = previous.height + level_data.height_diff.max;
                    height_min = Mathf.Clamp(height_min, HEIGHT_MIN, HEIGHT_MAX); // �ּҿ� �ִ밪 ���̸� ������ ����
                    height_max = Mathf.Clamp(height_max, HEIGHT_MIN, HEIGHT_MAX); // Mathf.clamp: min~max ������ ���� ��ȯ
                                                                                  // �ٴ� ������ �ּڰ�~�ִ� ������ ������ ��
                    current.height = Random.Range(height_min, height_max);
                    break;
            }
        }
    }*/


}
