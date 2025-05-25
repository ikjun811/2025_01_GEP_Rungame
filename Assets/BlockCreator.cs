using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockCreator : MonoBehaviour
{
    public GameObject[] blockPrefabs; // ����� ������ �迭
    private int block_count = 0;


    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void createBlock(Vector3 block_position)
    {
        // ������ �� ����� ����(����ΰ� �������ΰ�)�� ����
        int next_block_type = this.block_count % this.blockPrefabs.Length; // % : �������� ���ϴ� ������
                                                                           // ����� �����ϰ� go�� ����
        GameObject go = GameObject.Instantiate(this.blockPrefabs[next_block_type]) as GameObject;
        go.transform.position = block_position; // ����� ��ġ�� �̵�
        this.block_count++; // ����� ������ ����
    }
}