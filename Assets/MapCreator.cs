using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static LevelControl;

// Block Ŭ���� �߰�
public class Block
{
    // ����� ������ ��Ÿ���� ����ü
    public enum TYPE
    {
        NONE = -1, // ����
        FLOOR = 0, // ����
        HOLE, // ����
        NUM, // ����� �� ��������(��2)
    };
};

public class MapCreator : MonoBehaviour
{
    public static float BLOCK_WIDTH = 1.0f; // ����� ��
    public static float BLOCK_HEIGHT = 0.2f; // ����� ����
    public static int BLOCK_NUM_IN_SCREEN = 72;// ȭ�� ���� ���� ����� ����
                                               // ��Ͽ� ���� ������ ��Ƽ� �����ϴ� ����ü
                                               // (���� ���� ������ �ϳ��� ���� �� ���)
    private struct FloorBlock
    {
        public bool is_created; // ����� ��������°�
        public Vector3 position; // ����� ��ġ
    };
    private FloorBlock last_block; // �������� ������ ���
    private PlayerControl player = null;// scene���� Player�� ����
    private BlockCreator block_creator; // BlockCreator�� ����

    private LevelControl level_control = null;

    public TextAsset level_data_text = null;

    private GameRoot game_root = null; // �ɹ� ���� �߰�

    void Start()
    {
        this.player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerControl>();
        this.last_block.is_created = false;
        this.block_creator = this.gameObject.GetComponent<BlockCreator>();

        this.level_control = GetComponent<LevelControl>();

        // this.level_control.loadLevelData(this.level_data_text);

        this.player.level_control = this.level_control;

        this.game_root = this.gameObject.GetComponent<GameRoot>();
    }

    void Update()
    {
        // �÷��̾��� X��ġ�� ������
        float block_generate_x = this.player.transform.position.x;
        // �׸��� �뷫 �� ȭ�鸸ŭ ���������� �̵�
        // �� ��ġ�� ����� �����ϴ� ���� ��
        block_generate_x += BLOCK_WIDTH * ((float)BLOCK_NUM_IN_SCREEN + 1) / 3.0f * 2.0f;
        // �������� ���� ����� ��ġ�� ���� ������ ������
        while (this.last_block.position.x < block_generate_x)
        {
            // ����� ����
            this.create_floor_block();
        }
    }

    private void create_floor_block()
    {
        Vector3 block_position; // �������� ���� ����� ��ġ
        if (!this.last_block.is_created)
        { // last_block�� �������� ���� ���
          // ����� ��ġ�� �ϴ� Player�� ����
            block_position = this.player.transform.position;
            // �׷��� ���� ����� X ��ġ�� ȭ�� ���ݸ�ŭ �������� �̵�
            block_position.x -= BLOCK_WIDTH * ((float)BLOCK_NUM_IN_SCREEN / 3.0f);
            // ����� Y��ġ�� 0����
            block_position.y = 0.0f;
        }
        else
        { // last_block�� ������ ���
          // �̹��� ���� ����� ��ġ�� ������ ���� ��ϰ� ����
            block_position = this.last_block.position;
        }
        block_position.x += BLOCK_WIDTH; // ����� 1����ŭ ���������� �̵�
                                         // BlockCreator ��ũ��Ʈ�� createBlock()�޼ҵ忡 ������ ����
                                         // �Ʒ� �κ��� �ּ� ó��(Ȥ�� ����)
                                         // this.block_creator.createBlock(block_position);
                                         // �Ʒ� �κ��� �߰�.
        this.level_control.PrepareNextBlockPatternForMapCreator();
        LevelControl.CreationInfo current = this.level_control.current_block; // �غ�� ���� ��������
        block_position.y = current.height * BLOCK_HEIGHT;

        if (current.block_type == Block.TYPE.FLOOR)
        {
            this.block_creator.createBlock(block_position); // �ٴ��̸� ����
        }


        this.last_block.position = block_position;
        this.last_block.is_created = true;
    }

    public bool isDelete(GameObject block_object)
    {
        bool ret = false; // ��ȯ��
        float left_limit = this.player.transform.position.x
        - BLOCK_WIDTH * ((float)BLOCK_NUM_IN_SCREEN / 2.0f); // ���� ���� ��
                                                             // ����� ��ġ�� ���� ������ ������(����),
        if (block_object.transform.position.x < left_limit)
        {
            ret = true; // ��ȯ���� true(������� ����)��
        }
        return (ret); // ���� ����� ������
    }
}
