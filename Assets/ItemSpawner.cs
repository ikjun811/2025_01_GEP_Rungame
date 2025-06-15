using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemSpawner : MonoBehaviour
{
    [Header("아이템 프리팹")]
    public GameObject healthPackPrefab; // Inspector에서 체력 회복 아이템 프리팹 연결
    public GameObject stonePackPrefab;  // Inspector에서 돌멩이 회복 아이템 프리팹 연결
    public GameObject shieldPrefab;

    [Header("스폰 설정")]
    public float spawnIntervalMin = 8.0f;   // 최소 스폰 간격 (초)
    public float spawnIntervalMax = 15.0f;  // 최대 스폰 간격 (초)
    private float spawnTimer;               // 다음 스폰까지 남은 시간

    public float itemSpawnYPosition = 1.0f; // 아이템이 생성될 Y축 높이
    public float itemSpawnZMin = -3.0f;     // 아이템이 생성될 Z축 최소 범위
    public float itemSpawnZMax = 3.0f;      // 아이템이 생성될 Z축 최대 범위
    public float itemSpawnXOffset = 25f;    // 플레이어보다 얼마나 앞에서 생성될지 X축 거리

    private Transform playerTransform;      // 플레이어 위치 참조

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
            Debug.LogError("ItemSpawner: 'Player' 태그를 가진 오브젝트를 찾을 수 없습니다. 아이템 스폰이 제대로 작동하지 않을 수 있습니다.");
            // enabled = false; // 플레이어가 없으면 스포너 비활성화
        }
        ResetSpawnTimer();
    }

    // Update is called once per frame
    void Update()
    {
        if (playerTransform == null) // 플레이어 참조가 없으면 실행 중단
        {
            // 게임 중에 플레이어가 다시 생길 수 있다면 여기서 다시 찾기 시도 가능
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
        // Debug.Log($"다음 아이템 스폰까지: {spawnTimer:F2}초");
    }

    void SpawnRandomItem()
    {
        // 생성할 아이템 프리팹 목록 (확장 가능)
        List<GameObject> availableItems = new List<GameObject>();
        if (healthPackPrefab != null) availableItems.Add(healthPackPrefab);
        if (stonePackPrefab != null) availableItems.Add(stonePackPrefab);
        if (shieldPrefab != null) availableItems.Add(shieldPrefab);

        if (availableItems.Count == 0)
        {
            Debug.LogWarning("ItemSpawner: 스폰할 수 있는 아이템 프리팹이 없습니다.");
            return;
        }

        // 목록에서 무작위로 아이템 선택
        GameObject prefabToSpawn = availableItems[Random.Range(0, availableItems.Count)];

        // 스폰 위치 계산 (플레이어 앞, 무작위 Z)
        float randomZ = Random.Range(itemSpawnZMin, itemSpawnZMax);
        Vector3 spawnPosition = new Vector3(playerTransform.position.x + itemSpawnXOffset, itemSpawnYPosition, randomZ);

        Debug.Log($"아이템 '{prefabToSpawn.name}' 생성 위치: {spawnPosition}");
        Instantiate(prefabToSpawn, spawnPosition, Quaternion.identity); // Quaternion.identity는 회전 없음
    }

}
