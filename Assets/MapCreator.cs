using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static LevelControl;
using UnityEngine.SceneManagement;


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

    public TextAsset level_data_text = null;

    private GameRoot game_root = null; // 맴버 변수 추가

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded; // 이벤트 구독
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded; // 이벤트 구독 해제
    }


    void Start()
    {
        //this.player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerControl>();
        this.last_block.is_created = false;
        this.block_creator = this.gameObject.GetComponent<BlockCreator>();

        this.level_control = GetComponent<LevelControl>();

        // this.level_control.loadLevelData(this.level_data_text);

        //this.player.level_control = this.level_control;

        //this.game_root = this.gameObject.GetComponent<GameRoot>();
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        InitializeForNewScene();
    }

    void InitializeForNewScene()
    {
        // 1. 플레이어 참조 다시 찾기
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            this.player = playerObject.GetComponent<PlayerControl>();
            if (this.player != null)
            {
                Debug.Log("MapCreator: 새 씬에서 플레이어를 성공적으로 찾았습니다.");
            }
            else
            {
                Debug.LogError("MapCreator: 'Player' 태그를 가진 오브젝트에서 PlayerControl 컴포넌트를 찾지 못했습니다.");
                this.player = null; // 확실히 null로 설정
            }
        }
        else
        {
            Debug.LogError("MapCreator: 'Player' 태그를 가진 플레이어 오브젝트를 새 씬에서 찾지 못했습니다!");
            this.player = null; // 확실히 null로 설정
        }

        // 2. LevelControl 참조 다시 찾기
        LevelControl foundLevelControl = FindObjectOfType<LevelControl>(); // 임시 변수에 받아 확인

        if (foundLevelControl != null)
        {
            this.level_control = foundLevelControl; // 찾았으면 할당
            Debug.Log($"MapCreator: 새 씬에서 LevelControl을 찾았습니다: {this.level_control.gameObject.name}");
            if (this.player != null)
            {
                this.player.level_control = this.level_control;
            }
            this.level_control.InitializeBlockGeneration(); // 이제 호출 시도
        }
        else
        {
            Debug.LogError("MapCreator: 새 씬에서 LevelControl을 찾을 수 없습니다! (FindObjectOfType 실패)");
            this.level_control = null; // 명시적으로 null 처리
                                       // 아래의 this.level_control != null 조건에 의해 InitializeBlockGeneration 호출이 막힘
        }

        // GameRoot 참조는 GameRoot.Instance가 싱글톤이므로 비교적 안전합니다.
        if (GameRoot.Instance != null)
        {
            this.game_root = GameRoot.Instance;
        }
        else
        {
            Debug.LogWarning("MapCreator: GameRoot 인스턴스를 찾을 수 없습니다.");
        }

        // 3. 맵 생성 상태 초기화
        this.last_block.is_created = false;
        this.last_block.position = Vector3.zero; // last_block 위치도 초기화해주는 것이 안전할 수 있습니다.

        Debug.Log("MapCreator: 새 씬에 대한 초기화 완료 시도.");
    }

    void Update()
    {
        // 플레이어의 X위치를 가져옴
        float block_generate_x = this.player.transform.position.x;
        // 그리고 대략 반 화면만큼 오른쪽으로 이동
        // 이 위치가 블록을 생성하는 문턱 값
        block_generate_x += BLOCK_WIDTH * ((float)BLOCK_NUM_IN_SCREEN + 1) / 3.0f * 2.0f;

        while (this.last_block.position.x < block_generate_x)
        {

            this.create_floor_block();
        }
    }

    private void create_floor_block()
    {
        if (this.level_control == null)
        {
            Debug.LogError("create_floor_block: LevelControl 참조가 없습니다!");
            return;
        }
        this.level_control.PrepareNextBlockPatternForMapCreator();
        LevelControl.CreationInfo current = this.level_control.current_block;

        Vector3 block_position;
        if (!this.last_block.is_created)
        {
            block_position = this.player.transform.position; // 플레이어가 null이면 여기서도 오류 발생 가능
            block_position.x -= BLOCK_WIDTH * ((float)BLOCK_NUM_IN_SCREEN / 3.0f);


            block_position.y = 0.0f;


        }
        else
        {
            block_position = this.last_block.position;
        }
        block_position.x += BLOCK_WIDTH;

        block_position.y = current.height * BLOCK_HEIGHT;

        if (current.block_type == Block.TYPE.FLOOR)
        {
            if (this.block_creator != null) this.block_creator.createBlock(block_position);
            else Debug.LogError("create_floor_block: BlockCreator 참조가 없습니다!");
        }



        this.last_block.position = block_position;
        this.last_block.is_created = true;
    }

    public bool isDelete(GameObject block_object)
    {
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

    public void InitializeForNewStage()
    {
        Debug.Log("MapCreator: 새 스테이지를 위해 초기화 중...");
        this.last_block.is_created = false;
        this.last_block.position = Vector3.zero; // 또는 플레이어 시작 위치 기준으로 초기화

        // LevelControl이 이미 새 스테이지 설정으로 업데이트되었다고 가정하고,
        // LevelControl의 블록 생성 상태를 초기화합니다.
        if (level_control != null)
        {
            level_control.InitializeBlockGeneration();
        }
        else
        {
            Debug.LogError("MapCreator: LevelControl 참조가 없습니다. InitializeBlockGeneration 호출 불가.");
        }
        // TODO: 이전 스테이지에서 생성된 멀리 있는 블록들을 제거하는 로직이 필요할 수 있습니다.
        // 또는 맵의 시각적 테마를 변경하는 로직 등
    }

}
