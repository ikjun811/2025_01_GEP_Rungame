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
    private PlayerControl playerControl;

    // Start is called before the first frame update
    void Start()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

        playerTransform = playerObject.transform;
        playerControl = playerObject.GetComponent<PlayerControl>();

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
        if (gameObject.activeSelf && currentHp > 0)
        {
            fireTimer -= Time.deltaTime;
            if (fireTimer <= 0f)
            {
                FireWall();
                fireTimer = currentFireInterval; // 다음 발사 시간으로 타이머 리셋
            }
        }
    }

    void FireWall()
    {
        if (wallPrefab == null || firePoints == null || firePoints.Length == 0)
        {
            Debug.LogError("보스: Wall Prefab 또는 Fire Points가 설정되지 않았습니다!");
            return;
        }

        // 1. 발사 지점 랜덤 선택 (기존과 동일)
        int randomIndex = Random.Range(0, firePoints.Length);
        Transform selectedFirePoint = firePoints[randomIndex];
        if (selectedFirePoint == null)
        {
            Debug.LogError($"보스 공격 실패: Fire Points 배열의 {randomIndex}번 요소가 null입니다!");
            return;
        }

        // 2. ⭐ 벽의 이동 방향을 월드 X축 음의 방향으로 고정 ⭐
        Vector3 wallTravelDirection = Vector3.left; // Vector3.left는 (-1, 0, 0) 입니다.

        // 3. (선택적) 벽의 초기 회전값 설정: 벽이 이동 방향(Vector3.left)을 바라보도록 합니다.
        //    벽 프리팹의 로컬 Z축이 '앞'을 향하도록 디자인되었다고 가정합니다.
        Quaternion wallInitialRotation = Quaternion.LookRotation(wallTravelDirection);
        //    만약 벽이 구체처럼 방향성이 없거나, firePoint의 기본 회전값을 사용하고 싶다면
        //    wallInitialRotation = selectedFirePoint.rotation; 또는 Quaternion.identity; 로 설정할 수 있습니다.

        // 4. 벽 인스턴스 생성
        GameObject wallInstance = Instantiate(wallPrefab, selectedFirePoint.position, wallInitialRotation);
        // Debug.Log($"벽 생성됨: {wallInstance.name} at {selectedFirePoint.position}");

        // 5. 벽 이동 처리
        Rigidbody wallRb = wallInstance.GetComponent<Rigidbody>();
        if (wallRb != null)
        {
            // Rigidbody의 속도를 고정된 방향과 속도로 설정
            wallRb.velocity = wallTravelDirection * wallSpeed;
            // Debug.Log($"벽 Rigidbody 속도 설정: {wallRb.velocity}");
        }
        else
        {
            // Rigidbody가 없다면 WallMovement 스크립트로 이동 처리 시도
            WallMovement wallMovementScript = wallInstance.GetComponent<WallMovement>();
            if (wallMovementScript != null)
            {
                wallMovementScript.Initialize(wallTravelDirection, wallSpeed);
                // Debug.Log("벽 WallMovement 스크립트로 이동 초기화.");
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
        if (GameRoot.Instance != null)
        {
            GameRoot.Instance.NotifyStageCleared();
        }

        if (bossHealthSlider != null)
        {
            bossHealthSlider.gameObject.SetActive(false); 
        }
        gameObject.SetActive(false);
    }

    public void UpdateBossStats(float newMaxHp, float newFireInterval)
    {
        Debug.Log($"[확인필요_LOG] BossControl.UpdateBossStats: 호출됨! 전달받은 newMaxHp = {newMaxHp}, newFireInterval = {newFireInterval}. (호출 전 this.maxHp = {this.maxHp}, this.currentFireInterval = {this.currentFireInterval})");
        maxHp = newMaxHp;
        currentHp = maxHp;
        this.currentFireInterval = newFireInterval;
        fireTimer = this.currentFireInterval;
        UpdateBossHealthUI();
        Debug.Log($"[확인필요_LOG] BossControl.UpdateBossStats: 업데이트 후 this.maxHp = {this.maxHp}, this.currentFireInterval = {this.currentFireInterval}");
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
            // 보스 GameObject가 활성화되어 있고, 최대 체력이 0보다 클 때만 슬라이더를 표시/업데이트
            if (gameObject.activeInHierarchy && maxHp > 0)
            {
                bossHealthSlider.gameObject.SetActive(true);
                bossHealthSlider.value = currentHp / maxHp;
            }
            else
            {
                bossHealthSlider.gameObject.SetActive(false); 
            }
        }
    }

    public void ResetStateForNewStage()
    {
        Debug.Log($"[LOG] BossControl.ResetStateForNewStage: 호출됨. 진입 시 this.maxHp = {this.maxHp}, this.currentFireInterval = {this.currentFireInterval}");
        currentHp = maxHp;
        fireTimer = currentFireInterval;

        // 약점 위치 등 기타 상태 초기화
        if (weakPointObject != null && firePoints != null && firePoints.Length > 0)
        {
            currentWeakPointFirePointIndex = 0;
            weakPointObject.transform.position = firePoints[currentWeakPointFirePointIndex].position;
            weakPointMoveTimer = weakPointMoveInterval;
            weakPointObject.SetActive(true); // 약점도 활성화
        }

        gameObject.SetActive(true); // 보스 오브젝트 자체를 활성화
        UpdateBossHealthUI();       // UI 업데이트 (내부에서 슬라이더 활성화 포함)
        Debug.Log($"보스 상태 리셋 완료: HP={currentHp}/{maxHp}");
    }

}