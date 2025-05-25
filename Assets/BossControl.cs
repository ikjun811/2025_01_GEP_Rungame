using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BossControl : MonoBehaviour
{
    //위치 설정
    public Transform playerTransform;        // 플레이어의 Transform 참조
    public float followDistance = 15.0f;   // 플레이어보다 얼마나 앞에 있는지
    public float yPosition = 3.0f;         // 보스 고정 높이
    public float zPositionOffset = 0.0f;   // 플레이어의 Z축 위치에서 얼마나 벗어날지

    //체력 설정
    public float maxHp = 12f;             // 최대 체력, 스테이지 따라 변경
    private float currentHp;

    //공격 설정
    public GameObject wallPrefab;           // 발사할 벽의 프리팹
    public GameObject weakPointObject; //약점 프리팹
    public Transform[] firePoints; // 좌, 중, 우 발사, 약점 위치
    public float wallSpeed = 20.0f;         // 벽이 날아가는 속도
    public float initialFireInterval = 5.0f;// 초기 벽 발사 주기 (초) (스테이지별로 변경될 수 있음)
    private float currentFireInterval;      // 현재 적용된 발사 주기
    private float fireTimer;                // 다음 발사까지 남은 시간

    public float weakPointMoveInterval = 8.0f;  // 약점 이동 주기 (초)
    private float weakPointMoveTimer;
    private int currentWeakPointFirePointIndex = 0; // 현재 약점이 위치한 firePoint 인덱스

    public Slider bossHealthSlider;


    private LevelControl levelControl;

    // Start is called before the first frame update
    void Start()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

        playerTransform = playerObject.transform;

        levelControl = FindObjectOfType<LevelControl>();

        // 초기 스탯 설정 (LevelControl이 준비되었다면 여기서 스테이지 1 값으로 설정)
        currentHp = maxHp;
        currentFireInterval = initialFireInterval;
        fireTimer = currentFireInterval; // 첫 발사는 바로 또는 일정 시간 후

        UpdateBossHealthUI();

        if (weakPointObject != null && firePoints != null && firePoints.Length > 0)
        {
            // 초기 약점 위치를 첫 번째 firePoint로 설정 (또는 랜덤)
            currentWeakPointFirePointIndex = 0; // 또는 Random.Range(0, firePoints.Length);
            weakPointObject.transform.position = firePoints[currentWeakPointFirePointIndex].position;
            weakPointObject.SetActive(true); // 약점 활성화
        }
        else
        {
            Debug.LogError("보스 약점 또는 FirePoints가 설정되지 않았습니다!");
            if (weakPointObject != null) weakPointObject.SetActive(false);
        }
        weakPointMoveTimer = weakPointMoveInterval;

        // LevelControl에서 초기 보스 스탯을 받아오는 로직
        // UpdateBossStatsFromLevelControl(0); // 예: 0번 인덱스 스테이지 데이터로 초기화
    }

    // Update is called once per frame
    void Update()
    {
        if (playerTransform == null) return; // 플레이어가 없으면 행동 중단

        HandleMovement();
        HandleAttack();
        HandleWeakPointMovement();
    }

    void HandleMovement()
    {
        if (playerTransform != null)
        {
            // X 위치는 플레이어보다 항상 followDistance 앞에 있도록 설정
            float targetX = playerTransform.position.x + followDistance;
            // Y 위치는 고정된 yPosition 값 사용
            // Z 위치는 중앙(0)으로 고정 (또는 zPositionOffset이 0이면 플레이어와 같은 Z선상 유지)
            float targetZ = zPositionOffset; // 중앙 고정을 원하면 0f 로 설정

            transform.position = new Vector3(targetX, yPosition, targetZ);
        }
    }

    void HandleAttack()
    {
        fireTimer -= Time.deltaTime;
        if (fireTimer <= 0f)
        {
            FireWall();
            fireTimer = currentFireInterval; // 다음 발사 시간으로 타이머 리셋
        }
    }

    void FireWall()
    {
        if (wallPrefab == null || firePoints == null || firePoints.Length == 0)
        {
            Debug.LogError("보스: Wall Prefab 또는 Fire Points가 설정되지 않았습니다!");
            return;
        }

        // firePoints 배열에서 랜덤하게 하나 선택
        int randomIndex = Random.Range(0, firePoints.Length);
        Transform selectedFirePoint = firePoints[randomIndex];

        // 벽의 이동 방향 설정: 월드 X축 음의 방향 (Vector3.left)
        Vector3 wallTravelDirection = Vector3.left;

        Quaternion wallInitialRotation = Quaternion.LookRotation(wallTravelDirection);

        // 벽 인스턴스 생성
        GameObject wallInstance = Instantiate(wallPrefab, selectedFirePoint.position, wallInitialRotation);

        // 벽 오브젝트에 Rigidbody가 있고, 별도의 이동 스크립트가 없다면 여기서 속도 설정
        Rigidbody wallRb = wallInstance.GetComponent<Rigidbody>();
        if (wallRb != null)
        {
            wallRb.velocity = wallTravelDirection * wallSpeed;
        }
        else
        {
            // Rigidbody가 없다면, 벽 프리팹에 WallMovement.cs 같은 이동 스크립트를 만들어 붙이고,
            // 해당 스크립트에 방향과 속도를 전달하는 것이 좋습니다.
            WallMovement wallMovementScript = wallInstance.GetComponent<WallMovement>();
            if (wallMovementScript != null)
            {
                wallMovementScript.Initialize(wallTravelDirection, wallSpeed);
            }
            else
            {
                Debug.LogWarning("보스: 발사된 벽에 Rigidbody도 없고 WallMovement 스크립트도 없습니다. 벽이 움직이지 않을 수 있습니다.");
            }
        }
    }

    public void TakeDamage(float amount)
    {
        currentHp -= amount;
        currentHp = Mathf.Clamp(currentHp, 0, maxHp);
        UpdateBossHealthUI();
        Debug.Log($"보스 체력: {currentHp} / {maxHp}");

        if (currentHp <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log("보스 처치!");
        // (사망 이펙트, 아이템 드랍, 스테이지 클리어 등의 로직)
        if (bossHealthSlider != null) // ⭐ 보스 사망 시 체력바 처리 (선택적)
        {
            bossHealthSlider.value = 0;
        }
        Destroy(gameObject);
    }

    public void UpdateBossStats(float newMaxHp, float newFireInterval)
    {
        maxHp = newMaxHp;
        // 체력이 변경될 때 현재 체력도 비율에 맞게 조절하거나, 최대로 채울 수 있습니다.
        // 여기서는 단순히 현재 체력이 새 최대 체력을 넘지 않도록 합니다.
        currentHp = Mathf.Min(currentHp, maxHp);
        // 또는 currentHp = maxHp; // 새 스테이지 시작 시 체력을 최대로 회복

        currentFireInterval = newFireInterval;
        fireTimer = Mathf.Min(fireTimer, currentFireInterval); // 발사 주기가 짧아졌다면 타이머도 조절

        UpdateBossHealthUI();
        Debug.Log($"보스 스탯 업데이트됨 - HP: {maxHp}, 공격 주기: {currentFireInterval}초");
    }

    void HandleWeakPointMovement()
    {
        if (weakPointObject == null || !weakPointObject.activeSelf || firePoints == null || firePoints.Length == 0) return;

        weakPointMoveTimer -= Time.deltaTime;
        if (weakPointMoveTimer <= 0f)
        {
            // 다음 약점 위치를 랜덤하게 선택 (현재 위치와 다른 곳으로)
            int nextIndex = currentWeakPointFirePointIndex;
            if (firePoints.Length > 1) // firePoint가 2개 이상일 때만 이동 의미 있음
            {
                while (nextIndex == currentWeakPointFirePointIndex) // 이전 위치와 다른 곳 선택
                {
                    nextIndex = Random.Range(0, firePoints.Length);
                }
            }
            currentWeakPointFirePointIndex = nextIndex;
            weakPointObject.transform.position = firePoints[currentWeakPointFirePointIndex].position;
            // 약점 위치 변경 시 시각/청각적 효과 추가 가능

            weakPointMoveTimer = weakPointMoveInterval; // 타이머 리셋
            Debug.Log($"보스 약점 이동됨: firePoint[{currentWeakPointFirePointIndex}]");
        }
    }

    void UpdateBossHealthUI()
    {
        if (bossHealthSlider != null)
        {
            if (maxHp > 0) // maxHp가 0일 때 나누기 오류 방지
            {
                bossHealthSlider.value = currentHp / maxHp; // 현재 체력 비율로 슬라이더 값 설정
            }
            else
            {
                bossHealthSlider.value = 0; // maxHp가 0이면 체력도 0으로 표시
            }

            // (선택적) 보스가 활성화 상태일 때만 체력바를 표시하고 싶다면:
            // bossHealthSlider.gameObject.SetActive(gameObject.activeInHierarchy && currentHp > 0);
        }
        else
        {
            // Debug.LogWarning("BossHealthSlider가 할당되지 않았습니다."); // 필요하다면 경고 로그
        }
    }

}
