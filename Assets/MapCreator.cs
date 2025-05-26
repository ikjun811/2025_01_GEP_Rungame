using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using static LevelControl;

// Block 클래스 추가
public class Block
{
    // 블록의 종류를 나타내는 열거체
    public enum TYPE
    {
        NONE = -1, // 없음
        FLOOR = 0, // 마루
        HOLE, // 구멍
        NUM, // 블록이 몇 종류인지(＝2)
    };
};

public class MapCreator : MonoBehaviour
{
    public static float BLOCK_WIDTH = 1.0f; // 블록의 폭
    public static float BLOCK_HEIGHT = 0.2f; // 블록의 높이
    public static int BLOCK_NUM_IN_SCREEN = 72;// 화면 내에 들어가는 블록의 개수
                                               // 블록에 관한 정보를 모아서 관리하는 구조체
                                               // (여러 개의 정보를 하나로 묶을 때 사용)
    private struct FloorBlock
    {
        public bool is_created; // 블록이 만들어졌는가
        public Vector3 position; // 블록의 위치
    };


    private FloorBlock last_block; // 마지막에 생성한 블록
    private PlayerControl player = null;// scene상의 Player를 보관
    private BlockCreator block_creator; // BlockCreator를 보관

    private LevelControl level_control = null;

    //public TextAsset level_data_text = null;

    private GameRoot game_root = null; // 맴버 변수 추가

    public string gameStageSceneName = "GameStage";

    void Start()
    {
        //this.player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerControl>();
        this.last_block.is_created = false;
        this.block_creator = this.gameObject.GetComponent<BlockCreator>();

        if (GameRoot.Instance != null)
        {
            this.game_root = GameRoot.Instance;
        }

    }

    void Update()
    {
        if (SceneManager.GetActiveScene().name != gameStageSceneName || player == null)
        {
            return;
        }

        // 플레이어의 X위치를 가져옴
        float block_generate_x = this.player.transform.position.x;
        // 그리고 대략 반 화면만큼 오른쪽으로 이동
        // 이 위치가 블록을 생성하는 문턱 값
        block_generate_x += BLOCK_WIDTH * ((float)BLOCK_NUM_IN_SCREEN + 1) / 3.0f * 2.0f;
        // 마지막에 만든 블록의 위치가 문턱 값보다 작으면
        while (this.last_block.position.x < block_generate_x)
        {
            // 블록을 만듬
            this.create_floor_block();
        }
    }

    private void create_floor_block()
    {
        if (player == null || level_control == null)
        {
            return;
        }

        Vector3 block_position; // 이제부터 만들 블록의 위치
        if (!this.last_block.is_created)
        { // last_block이 생성되지 않은 경우
          // 블록의 위치를 일단 Player와 같게
            block_position = this.player.transform.position;
            // 그러고 나서 블록의 X 위치를 화면 절반만큼 왼쪽으로 이동
            block_position.x -= BLOCK_WIDTH * ((float)BLOCK_NUM_IN_SCREEN / 3.0f);
            // 블록의 Y위치는 0으로
            block_position.y = 0.0f;
        }
        else
        { // last_block이 생성된 경우
          // 이번에 만들 블록의 위치를 직전에 만든 블록과 같게
            block_position = this.last_block.position;
        }
        block_position.x += BLOCK_WIDTH; // 블록을 1블럭만큼 오른쪽으로 이동
                                         // BlockCreator 스크립트의 createBlock()메소드에 생성을 지시
                                         // 아래 부분을 주석 처리(혹은 삭제)
                                         // this.block_creator.createBlock(block_position);
                                         // 아래 부분을 추가.
        this.level_control.PrepareNextBlockPatternForMapCreator();
        LevelControl.CreationInfo current = this.level_control.current_block; // 준비된 정보 가져오기
        block_position.y = current.height * BLOCK_HEIGHT;

        if (current.block_type == Block.TYPE.FLOOR)
        {
            this.block_creator.createBlock(block_position); // 바닥이면 생성
        }


        this.last_block.position = block_position;
        this.last_block.is_created = true;
    }

    public bool isDelete(GameObject block_object)
    {
        if (player == null)
        {
            return false;
        }


        bool ret = false; // 반환값
        float left_limit = this.player.transform.position.x
        - BLOCK_WIDTH * ((float)BLOCK_NUM_IN_SCREEN / 2.0f); // 삭제 문턱 값
                                                             // 블록의 위치가 문턱 값보다 작으면(왼쪽),
        if (block_object.transform.position.x < left_limit)
        {
            ret = true; // 반환값을 true(사라져도 좋다)로
        }
        return (ret); // 판정 결과를 돌려줌
    }

    public void InitializeForNewStage(LevelControl lcInstanceFromGameRoot)
    {
        Debug.Log("MapCreator: InitializeForNewStage가 LevelControl 참조와 함께 호출됨.");
        this.level_control = lcInstanceFromGameRoot; // 전달받은 참조 사용

        // 플레이어 참조 다시 찾기 (이 부분은 기존 로직 유지 또는 GameRoot로부터 전달받을 수도 있음)
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            this.player = playerObject.GetComponent<PlayerControl>();
            if (this.player != null)
            {
                Debug.Log("MapCreator: 새 씬(스테이지)에서 플레이어를 찾았습니다.");
                if (this.level_control != null) // level_control이 유효하면 PlayerControl에도 설정
                {
                    this.player.level_control = this.level_control;
                }
            }
            else
            {
                Debug.LogError("MapCreator: 'Player' 태그 오브젝트에서 PlayerControl 컴포넌트를 찾지 못했습니다.");
                this.player = null;
            }
        }
        else
        {
            Debug.LogError("MapCreator: 'Player' 태그를 가진 플레이어 오브젝트를 새 씬(스테이지)에서 찾지 못했습니다!");
            this.player = null;
        }

        // GameRoot 참조 설정 (기존대로 GameRoot.Instance 사용 가능)
        if (GameRoot.Instance != null)
        {
            this.game_root = GameRoot.Instance;
        }
        else
        {
            Debug.LogWarning("MapCreator: GameRoot 인스턴스를 찾을 수 없습니다.");
        }

        // 맵 생성 상태 초기화
        this.last_block.is_created = false;
        this.last_block.position = Vector3.zero; // 초기 기준 위치 설정

        if (this.level_control != null)
        {
            Debug.Log("MapCreator: LevelControl 참조가 유효합니다. InitializeBlockGeneration()을 호출합니다.");
            this.level_control.InitializeBlockGeneration();
        }
        else
        {
            // 이 로그가 사용자에게 현재 나타나는 로그입니다.
            // GameRoot가 전달한 LevelControl 인스턴스가 null이라는 의미가 됩니다.
            Debug.LogError("MapCreator: LevelControl 참조가 없습니다. InitializeBlockGeneration 호출 불가.");
        }
        Debug.Log("MapCreator: 새 스테이지에 대한 초기화 완료 시도.");
    }

}