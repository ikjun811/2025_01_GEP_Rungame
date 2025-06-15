using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemSpawner : MonoBehaviour
{
    [Header("������ ������")]
    public GameObject healthPackPrefab; // Inspector���� ü�� ȸ�� ������ ������ ����
    public GameObject stonePackPrefab;  // Inspector���� ������ ȸ�� ������ ������ ����
    public GameObject shieldPrefab;

    [Header("���� ����")]
    public float spawnIntervalMin = 8.0f;   // �ּ� ���� ���� (��)
    public float spawnIntervalMax = 15.0f;  // �ִ� ���� ���� (��)
    private float spawnTimer;               // ���� �������� ���� �ð�

    public float itemSpawnYPosition = 1.0f; // �������� ������ Y�� ����
    public float itemSpawnZMin = -3.0f;     // �������� ������ Z�� �ּ� ����
    public float itemSpawnZMax = 3.0f;      // �������� ������ Z�� �ִ� ����
    public float itemSpawnXOffset = 25f;    // �÷��̾�� �󸶳� �տ��� �������� X�� �Ÿ�

    private Transform playerTransform;      // �÷��̾� ��ġ ����

    // Start is called before the first frame update
    void Start()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            playerTransform = playerObject.transform;
        }
        else
        {
            Debug.LogError("ItemSpawner: 'Player' �±׸� ���� ������Ʈ�� ã�� �� �����ϴ�. ������ ������ ����� �۵����� ���� �� �ֽ��ϴ�.");
            // enabled = false; // �÷��̾ ������ ������ ��Ȱ��ȭ
        }
        ResetSpawnTimer();
    }

    // Update is called once per frame
    void Update()
    {
        if (playerTransform == null) // �÷��̾� ������ ������ ���� �ߴ�
        {
            // ���� �߿� �÷��̾ �ٽ� ���� �� �ִٸ� ���⼭ �ٽ� ã�� �õ� ����
            // GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            // if (playerObject != null) playerTransform = playerObject.transform;
            // else return;
            return;
        }

        spawnTimer -= Time.deltaTime;
        if (spawnTimer <= 0)
        {
            SpawnRandomItem();
            ResetSpawnTimer();
        }
    }

    void ResetSpawnTimer()
    {
        spawnTimer = Random.Range(spawnIntervalMin, spawnIntervalMax);
        // Debug.Log($"���� ������ ��������: {spawnTimer:F2}��");
    }

    void SpawnRandomItem()
    {
        // ������ ������ ������ ��� (Ȯ�� ����)
        List<GameObject> availableItems = new List<GameObject>();
        if (healthPackPrefab != null) availableItems.Add(healthPackPrefab);
        if (stonePackPrefab != null) availableItems.Add(stonePackPrefab);
        if (shieldPrefab != null) availableItems.Add(shieldPrefab);

        if (availableItems.Count == 0)
        {
            Debug.LogWarning("ItemSpawner: ������ �� �ִ� ������ �������� �����ϴ�.");
            return;
        }

        // ��Ͽ��� �������� ������ ����
        GameObject prefabToSpawn = availableItems[Random.Range(0, availableItems.Count)];

        // ���� ��ġ ��� (�÷��̾� ��, ������ Z)
        float randomZ = Random.Range(itemSpawnZMin, itemSpawnZMax);
        Vector3 spawnPosition = new Vector3(playerTransform.position.x + itemSpawnXOffset, itemSpawnYPosition, randomZ);

        Debug.Log($"������ '{prefabToSpawn.name}' ���� ��ġ: {spawnPosition}");
        Instantiate(prefabToSpawn, spawnPosition, Quaternion.identity); // Quaternion.identity�� ȸ�� ����
    }

}
