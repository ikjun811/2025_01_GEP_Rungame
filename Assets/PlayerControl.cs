using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerControl : MonoBehaviour
{
    public static float ACCELERATION = 30.0f; // 가속도
    public static float SPEED_MIN = 12.0f; // 속도의 최솟값
    public static float SPEED_MAX = 18.0f; // 속도의 최댓값
    public static float JUMP_HEIGHT_MAX = 6.0f; // 점프 높이
    public static float JUMP_KEY_RELEASE_REDUCE = 0.5f; // 점프 감속도
    public static float DOUBLE_JUMP_HEIGHT_REDUCE = 0.7f; //더블 점프 감속도
    public float floatStaminaCostRate = 30f; // 부양 중 초당 스테미너 소모량
    public float minFloatStamina = 10f; // 부양 가능 최소 스테미너


    public float maxStamina = 100f;         // 최대 스태미너
    public float currentStamina = 100f;       // 현재 스태미너
    public float staminaJumpCost = 5f;       // 점프 스태미너 소모량
    public float staminaDoubleJumpCost = 5f; // 더블 점프 스테미너 소모량
    public float staminaRecoveryRate = 10f;   // 초당 스태미너 회복량
    public float staminaRecoveryDelay = 0.1f;  // 회복 딜레이 시간 (착지 후 바로 회복 시작)
    //private float staminaRecoveryTimer = 0f;   // 스태미너 회복 타이머

    private bool isFloating = false;         // 부양 중인지 여부
    private bool hasDoubleJumped = false;    // 더블 점프를 이미 수행했는지 여부
    public float maxFloatYSpeed = 2f;
    public float upwardForceValue = 0.1f; //부양에 가하는 힘

    public Slider staminaSlider; //스테미나 확인용 슬라이드

    public float dashDistance = 100f; // 돌진 거리
    public float dashSpeedMultiplier = 5f; // 돌진 속도 배율
    //public float dashStaminaCost = 20f; // 돌진 시 스테미너 소모량
    public float dashCoolDownTime = 1f; // 돌진 쿨타임

    private bool isDashing = false;          // 돌진 중인지 여부
    private float dashCoolDownTimer = 0f;    // 돌진 쿨타임 타이머

    public float dashDuration = 0.2f; // 예: 0.2초 동안 돌진 상태 유지
    private float dashTimer = 0.0f;   // 돌진 상태 내부 타이머

    public float lateralSpeed = 8.0f; // 좌우 이동 속도
    public float minZPos = -5f; // 왼쪽 이동 제한
    public float maxZPos = 5f;  // 오른쪽 이동 제한

    [Header("플레이어 체력")]
    public float maxPlayerHp = 100f;
    private float currentPlayerHp;
    public Slider playerHealthSlider;

    [Header("투척용 돌멩이")]
    private bool hasStone = false;
    public GameObject stoneProjectilePrefab;
    public Transform stoneSpawnPoint;       // 돌멩이가 발사될 위치 
    //public float stoneThrowForce = 50f;   // 돌멩이 발사 힘
    public float additionalStoneSpeed = 15f;
    public GameObject heldStoneVisual;

    [Header("플레이어 제약")]
    public float maxRiseSpeed = 8.0f;  // 평상시 상승 속도 제한
    public float emergencyMaxHeight = 15.0f; // 비상용 최대 높이 제한

    [Header("돌진 쿨타임 UI")]
    public Slider dashCooldownSlider;

    [Header("상태 아이콘 UI")]
    public GameObject stoneLoadedIcon; // Inspector에서 돌멩이 아이콘 연결
    public GameObject shieldActiveIcon;  // Inspector에서 실드 아이콘 연결

    [Header("방어막 설정")]
    public GameObject shieldVisualEffect; // 방어막 이펙트 오브젝트
    private bool isShieldActive = false;  // 현재 방어막 활성화 상태


    public enum STEP
    { // Player의 각종 상태를 나타내는 자료형 (열거체)
        NONE = -1, // 상태정보 없음
        RUN = 0, // 달림
        JUMP, // 점프
        MISS, // 실패
        DASH, // 돌진
        NUM, // 상태가 몇 종류 있는지 보여줌(=5)
    };

    public STEP step = STEP.NONE; // Player의 현재 상태
    public STEP next_step = STEP.NONE; // Player의 다음 상태
    public float step_timer = 0.0f; // 경과 시간
    private bool is_landed = false; // 착지했는가
    private bool is_colided = false; // 뭔가와 충돌했는가
    private bool is_key_released = false; // 버튼이 떨어졌는가
    //private Vector3 dashStartPos;        // 돌진 시작 위치

    public static float NARAKU_HEIGHT = -5.0f; //게임 오버 한계선

    // LevelControl과 연계하기 위해 필요
    public float current_speed = 0.0f; // 현재 속도
    public LevelControl level_control = null; // LevelControl이 저장

    private float click_timer = 1.0f; // 버튼이 눌린 후의 시간
    private float CLICK_GRACE_TIME = 0.5f; // 점프하고 싶은 의사를 받아들일 시간


    // Start is called before the first frame update
    void Start()
    {
        this.next_step = STEP.RUN;
        currentStamina = maxStamina;
        isFloating = false;
        hasDoubleJumped = false;
        isDashing = false;
        dashCoolDownTimer = 0f;
        currentPlayerHp = maxPlayerHp;
        hasStone = false;
        if (heldStoneVisual != null)
        {
            heldStoneVisual.SetActive(hasStone); // 초기 상태에 맞춰 비활성화
        }

        dashCooldownSlider.gameObject.SetActive(false);
        UpdatePlayerHealthUI();

        if (shieldVisualEffect != null)
        {
            shieldVisualEffect.SetActive(false); // 게임 시작 시 방어막 비활성화
        }
        isShieldActive = false;

        if (stoneLoadedIcon != null) stoneLoadedIcon.SetActive(false);
        if (shieldActiveIcon != null) shieldActiveIcon.SetActive(false);

    }

    // Update is called once per frame
    void Update()
    {
        //미사용
        // this.transform.Translate(new Vector3(0.0f, 0.0f, 3.0f * Time.deltaTime));

        Rigidbody rb = this.GetComponent<Rigidbody>();
        Vector3 velocity = rb.velocity; // 속도를 설정


        // 아래 현재 속도를 가져오는 메서드 호출 추가
        this.current_speed = this.level_control.getPlayerSpeed();

        this.check_landed(); // 착지 상태인지 체크

        // 스태미너 회복 로직
        if (currentStamina < maxStamina)
        {
            currentStamina += staminaRecoveryRate * Time.deltaTime;
            currentStamina = Mathf.Clamp(currentStamina, 0f, maxStamina); // 최대치 제한
            UpdateStaminaUI();
        }

        if (is_landed)
        {
            isFloating = false;       // 착지하면 부양 종료
            hasDoubleJumped = false;  // 착지하면 더블 점프 수행 상태 초기화

        }


        // 돌진 쿨타임 관리
        if (dashCoolDownTimer > 0)
        {
            dashCoolDownTimer -= Time.deltaTime;
        }

        // 부양 로직
        if (isFloating && step == STEP.JUMP && hasDoubleJumped)
        {
            if (Input.GetKey(KeyCode.Space) && currentStamina > minFloatStamina)
            {
  
                Vector3 vel = rb.velocity;

                // 현재 y 속도가 최대 부양 속도(maxFloatYSpeed)보다 낮을 때만 부드럽게 상승
                if (vel.y < maxFloatYSpeed)
                {
                    // 목표 속도까지 서서히 도달하도록 힘을 가함
                    rb.AddForce(Vector3.up * upwardForceValue, ForceMode.Acceleration);
                }
                else
                {
                    // 이미 최대 속도에 도달했거나 그보다 빠르면, 속도를 강제로 유지
                    vel.y = maxFloatYSpeed;
                    rb.velocity = vel;
                }

                // 스태미나 소모
                float staminaToConsume = floatStaminaCostRate * Time.deltaTime;
                currentStamina -= staminaToConsume;
                currentStamina = Mathf.Clamp(currentStamina, 0f, maxStamina);
            }

            UpdateStaminaUI();
        }

        if (velocity.y > maxRiseSpeed)
        {
            velocity.y = maxRiseSpeed;
        }


        if (hasStone && Input.GetMouseButtonDown(0)) // 마우스 좌클릭 그리고 돌멩이가 있을때
        {
            ThrowStone();
        }



        if (dashCoolDownTimer > 0)
        {
            dashCoolDownTimer -= Time.deltaTime;

            // 쿨타임이 도는 동안 슬라이더 업데이트
            if (dashCooldownSlider != null)
            {
               
                dashCooldownSlider.value = dashCoolDownTimer / dashCoolDownTime;
            }
            
        }
        else if (dashCooldownSlider != null && dashCooldownSlider.gameObject.activeInHierarchy)
        {

            dashCoolDownTimer = 0; // 음수가 되지 않도록 0으로 고정
            dashCooldownSlider.gameObject.SetActive(false); // 슬라이더 비활성화
        }



        if (Input.GetMouseButtonDown(1) &&      // 우클릭
        this.step != STEP.DASH &&           // 현재 돌진 상태가 아닐 때 (중복 방지)
        dashCoolDownTimer <= 0f &&          // 쿨타임이 끝났을 때
        (this.step == STEP.RUN || this.step == STEP.JUMP)) // 달리기 또는 점프 중에만
        {

            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.PlaySfx(SoundManager.Instance.playerDashSfx);
            }

            dashCoolDownTimer = dashCoolDownTime; // 쿨타임 시작

            if (dashCooldownSlider != null)
            {
                dashCooldownSlider.gameObject.SetActive(true);
                dashCooldownSlider.value = 1.0f; 
            }
            this.isDashing = true; 
               
            this.next_step = STEP.DASH;
        }


        switch (this.step)
        {
            case STEP.RUN:
            case STEP.JUMP:
            case STEP.DASH:
                // 현재 위치가 한계치보다 아래면,
                if (this.transform.position.y < NARAKU_HEIGHT)
                {
                    this.next_step = STEP.MISS; // '실패' 상태
                    GameRoot.Instance.NotifyGameOver();
                }
                break;
        }

        this.step_timer += Time.deltaTime; // 경과 시간을 진행

        if (Input.GetKeyDown(KeyCode.Space))
        { // 버튼이 눌렸으면,
            this.click_timer = 0.0f; // 타이머를 리셋
        }
        else
        {
            if (this.click_timer >= 0.0f)
            { // 그렇지 않으면,
                this.click_timer += Time.deltaTime; // 경과 시간을 더함
            }
        }

        // 다음 상태가 정해져 있지 않으면 상태의 변화를 조사
        if (this.next_step == STEP.NONE)
        {
            switch (this.step)
            { // Player의 현재 상태로 분기
                case STEP.RUN: //달리는중
                    if (0.0f <= this.click_timer && this.click_timer <= CLICK_GRACE_TIME)
                    { // click_timer가 0이상, CLICK_GRACE_TIME이하라면,
                        if (this.is_landed)
                        { // 착지했다면,
                          // 스태미너가 점프 비용보다 많거나 같으면 점프
                            if (currentStamina >= staminaJumpCost)
                            {
                                currentStamina -= staminaJumpCost;
                                UpdateStaminaUI();
                                this.click_timer = -1.0f; // 버튼이 눌려있지 않음을 나타내는 -1.0f로
                                velocity.y = Mathf.Sqrt(2.0f * 9.8f * JUMP_HEIGHT_MAX);
                                this.is_key_released = false; // 키 릴리즈 플래그 초기화
                                this.next_step = STEP.JUMP; // 점프 상태로
                            }
                            // 스태미너가 부족하지만 점프를 허용
                            else if (currentStamina < staminaJumpCost && currentStamina > 0)
                            {
                                currentStamina = 0; // 남은 스태미너 모두 소모
                                UpdateStaminaUI();
                                this.click_timer = -1.0f; // 버튼이 눌려있지 않음을 나타내는 -1.0f로
                                velocity.y = Mathf.Sqrt(2.0f * 9.8f * JUMP_HEIGHT_MAX);
                                this.is_key_released = false; // 키 릴리즈 플래그 초기화
                                this.next_step = STEP.JUMP; // 점프 상태로
                            }
                            else if (currentStamina <= 0)
                            {
                                // 스태미너 부족으로 점프 못하므로 패스
                            }
                        }
                    }
                    break;
                case STEP.JUMP: // 점프 중일 때
                    if (!this.is_landed && !hasDoubleJumped && (0.0f <= this.click_timer && this.click_timer <= CLICK_GRACE_TIME))
                    {
                        // 공중에 있고 아직 더블 점프를 안 했으며 점프 입력이 있을 때
                        if (currentStamina >= staminaDoubleJumpCost)
                        {
                            currentStamina -= staminaDoubleJumpCost;
                            velocity.y = Mathf.Sqrt(2.0f * 9.8f * JUMP_HEIGHT_MAX * DOUBLE_JUMP_HEIGHT_REDUCE); // 더블 점프 높이 적용
                            this.hasDoubleJumped = true;     // 더블 점프 사용함
                            this.isFloating = true;          // 더블 점프 후 부양 가능하도록 설정
                            this.is_key_released = false;    // 새로운 상승 동작이므로 키 릴리즈 플래그 리셋
                            this.click_timer = -1.0f;        // 입력 처리됨
                        }
                        else if (currentStamina < staminaDoubleJumpCost && currentStamina > 0)
                        {
                            currentStamina = 0;
                            velocity.y = Mathf.Sqrt(2.0f * 9.8f * JUMP_HEIGHT_MAX * DOUBLE_JUMP_HEIGHT_REDUCE);
                            this.hasDoubleJumped = true;
                            this.isFloating = true;
                            this.is_key_released = false;
                            this.click_timer = -1.0f;
                        }
                        UpdateStaminaUI();
                    }
                    else if (this.is_landed)
                    {

                        this.next_step = STEP.RUN;
                    }
                    break;
            }
        }
        // '다음 정보'가 '상태 정보 없음'이 아닌 동안(상태가 변할 때만)
        while (this.next_step != STEP.NONE)
        {
            this.step = this.next_step; // '현재 상태'를 '다음 상태'로 갱신
            this.next_step = STEP.NONE; // '다음 상태'를 '상태 없음'으로 변경

            switch (this.step)
            { // 갱신된 '현재 상태'가
                case STEP.JUMP: // '점프’일 때,

                    // '버튼이 떨어졌음을 나타내는 플래그'를 클리어
                    this.is_key_released = false;
                    break;
                case STEP.DASH: // ⭐ 돌진 상태 진입 시 처리
                    this.dashTimer = 0.0f;  // 돌진 내부 타이머 초기화
                    this.isDashing = true;
                    this.is_landed = false; // 돌진 중에는 착지 상태를 잠시 해제 (공중 돌진 가능성)
                    break;
            }
            // 상태가 변했으므로 경과 시간을 제로로 리셋
            this.step_timer = 0.0f;
        }
        // 상태별로 매 프레임 갱신 처리
        switch (this.step)
        {
            case STEP.RUN: // 달리는 중일 때,
                           //속도 증가
                velocity.x = this.current_speed;
                if (is_landed) // RUN 상태이고 착지했다면
                {
                    // 미세한 Y축 상승을 억제하여 바닥에 붙어 있도록 함
                    if (velocity.y > 0.01f) // 아주 작은 양수 속도라도 있다면 (튕겨오른 직후)
                    {
                        velocity.y = 0f; // 강제로 0으로 만들거나, 작은 음수 값으로 설정
                                         // velocity.y = -0.1f; (바닥으로 살짝 누르는 효과)
                    }
                }

                break;
            case STEP.JUMP: // 점프 중일 때
                velocity.x = this.current_speed;
                do
                {
                    // '버튼이 떨어진 순간'이 아니면
                    if (!Input.GetKeyUp(KeyCode.Space))
                    {
                        break; // 아무것도 하지 않고 루프를 빠져나감
                    }
                    // 이미 감속된 상태면(두 번이상 감속하지 않도록)
                    if (this.is_key_released)
                    {
                        break; // 아무것도 하지 않고 루프를 빠져나감
                    }
                    // 상하방향 속도가 0 이하면(하강 중이라면)
                    if (velocity.y <= 0.0f)
                    {
                        break; // 아무것도 하지 않고 루프를 빠져나감
                    }
                    // 버튼이 떨어져 있고 상승 중이라면, 감속 시작
                    // 점프의 상승은 여기서 끝
                    velocity.y *= JUMP_KEY_RELEASE_REDUCE;
                    this.is_key_released = true;
                } while (false);
                break;
            case STEP.MISS:
                // 가속도(ACCELERATION)를 빼서 Player의 속도를 느리게
                velocity.x -= PlayerControl.ACCELERATION * Time.deltaTime;
                if (velocity.x < 0.0f)
                { // Player의 속도가 마이너스면,
                    velocity.x = 0.0f; // 0으로
                }
                break;
            case STEP.DASH: // ⭐ 돌진 상태일 때 매 프레임 처리
                dashTimer += Time.deltaTime;
                if (dashTimer < dashDuration)
                {
                    // 돌진은 매우 빠른 속도로 X축 이동
                    velocity.x = this.current_speed * dashSpeedMultiplier;
                    velocity.y = 0f; // 현재 Y축 속도 0 강제
                }
                else
                {
                    // 돌진 종료
                    this.isDashing = false;

                    velocity.x = this.current_speed;

                    if (this.is_landed)
                    {
                        this.next_step = STEP.RUN;
                    }
                    else
                    {
                        // 공중에서 돌진이 끝났다면, 일반 점프/낙하 상태로 돌아감
                        this.next_step = STEP.JUMP;
                    }
                }
                break;
        }

        float lateralInput = 0f;
        if (Input.GetKey((KeyCode)97)) // 'A' 키 누르면 왼쪽으로
        {
            lateralInput = 1f;
        }
        else if (Input.GetKey((KeyCode)100)) // 'D' 키 누르면 오른쪽으로
        {
            lateralInput = -1f;
        }
        velocity.z = lateralInput * lateralSpeed;

        Vector3 clampedPos = rb.position;
        clampedPos.z = Mathf.Clamp(rb.position.z, minZPos, maxZPos);
        rb.position = clampedPos; //좌우 이동 제한

        // Rigidbody의 속도를 위에서 구한 속도로 갱신 (이 행은 상태에 관계없이 매번 실행)
        rb.velocity = velocity;
    }

    private void check_landed()
    { // 착지했는지 조사
        if (this.step == STEP.JUMP && this.step_timer < 0.1f) //잠깐 딜레이
        {
            this.is_landed = false;
            return; // check_landed 함수를 여기서 종료
        }
        do
        {
            Vector3 s = this.transform.position; // Player의 현재 위치
            Vector3 e = s + Vector3.down * 1.0f; // s부터 아래로 1로 이동한 위치
            RaycastHit hit;
            if (!Physics.Linecast(s, e, out hit))
            { // s부터 e 사이에 아무것도 없을 때
                break; // 아무것도 하지 않고 do~while 루프를 빠져나감(탈출구로)
            }
            // s부터 e 사이에 뭔가 있을 때 아래의 처리가 실행
            if (this.step == STEP.JUMP)
            { // 현재, 점프, 더블점프 상태라면
                if (this.step_timer < Time.deltaTime * 0.1f)
                { // 경과 시간이 0.1f 미만이라면
                    break; // 아무것도 하지 않고 do~while 루프를 빠져나감(탈출구로)
                }
            }
            // s부터 e 사이에 뭔가 있고 JUMP 직후가 아닐 때만 아래가 실행
            this.is_landed = true;
        } while (false);
        // 루프의 탈출구
    }

    void UpdateStaminaUI()
    {
        if (staminaSlider != null)
        {
            staminaSlider.value = currentStamina;
        }
    }

    public bool IsPlayerDashing()
    {
        return this.step == STEP.DASH;
    }

    public void TakeDamage(float amount)
    {
        if (this.step == STEP.MISS || this.next_step == STEP.MISS) return; // 이미 죽은 상태면 데미지 안 받음

        if (isShieldActive)
        {
            DeactivateShield(); // 방어막 비활성화
            Debug.Log("방어막이 데미지를 흡수했습니다!");
            return; // 데미지 처리 로직을 실행하지 않고 함수 종료
        }

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySfx(SoundManager.Instance.playerHitSfx);
        }

        currentPlayerHp -= amount;
        currentPlayerHp = Mathf.Clamp(currentPlayerHp, 0, maxPlayerHp);
        UpdatePlayerHealthUI();

        if (currentPlayerHp <= 0)
        {
            Die();
        }
    }

    void UpdatePlayerHealthUI()
    {
        if (playerHealthSlider != null)
        {
            playerHealthSlider.value = currentPlayerHp / maxPlayerHp; // 슬라이더 값은 0~1 사이
        }
    }

    void Die()
    {
        if (this.step == STEP.MISS || this.next_step == STEP.MISS)
        {
            return;
        }

        this.next_step = STEP.MISS;
        if (GameRoot.Instance != null)
        {
            GameRoot.Instance.NotifyGameOver(); // GameRoot에 게임 오버 알림
        }
        else
        {
            Debug.LogError("PlayerControl.Die(): GameRoot.Instance를 찾을 수 없습니다. 게임 오버 처리를 할 수 없습니다.");
        }

    }

    public void CollectStone()
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySfx(SoundManager.Instance.stoneAcquireSfx);
        }


        if (!hasStone) // 돌멩이가 없을 때만 획득
        {
  
            hasStone = true;
            heldStoneVisual.SetActive(true);

            if (stoneLoadedIcon != null)
            {
                stoneLoadedIcon.SetActive(true);
            }
            Debug.Log("돌멩이 획득!");
        }
    }

    void ThrowStone()
    {
        if (stoneProjectilePrefab == null || stoneSpawnPoint == null)
        {
            Debug.LogError("돌멩이 프리팹 또는 발사 위치가 설정되지 않았습니다!");
            return;
        }

        Debug.Log("돌멩이 투척!");

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySfx(SoundManager.Instance.stoneThrowSfx);
        }
        // 1. 발사 방향 결정
        Vector3 throwDirection = Vector3.right; // (1, 0, 0)

        // 2. 돌멩이 초기 회전 설정: 발사 방향을 바라보도록 합니다.
        Quaternion projectileRotation = Quaternion.LookRotation(throwDirection);
        //    또는, stoneSpawnPoint의 회전을 사용하고 싶다면:
        //    Quaternion projectileRotation = stoneSpawnPoint.rotation;
        //    Vector3 throwDirection = stoneSpawnPoint.forward; // 이 경우 스폰포인트 방향을 사용

        // 3. 돌멩이 인스턴스 생성
        GameObject stoneInstance = Instantiate(stoneProjectilePrefab, stoneSpawnPoint.position, projectileRotation);
        Rigidbody stoneRb = stoneInstance.GetComponent<Rigidbody>();
        if (stoneRb != null)
        {
            // stoneSpawnPoint의 앞쪽 방향으로 발사
            float targetStoneSpeed = this.current_speed + additionalStoneSpeed;
            stoneRb.velocity = throwDirection * targetStoneSpeed;


        }
        else
        {
            StoneProjectile projScript = stoneInstance.GetComponent<StoneProjectile>();
            if (projScript != null)
            {
                float targetStoneSpeed = this.current_speed + additionalStoneSpeed;
                // projScript.InitializeMovement(throwDirection, targetStoneSpeed); // StoneProjectile에 이런 함수 필요
            }
        }

        hasStone = false; // 돌멩이 사용
        heldStoneVisual.SetActive(false);

        if (stoneLoadedIcon != null)
        {
            stoneLoadedIcon.SetActive(false);
        }

    }

    public void RestoreHealth()
    {
        currentPlayerHp = maxPlayerHp;
        UpdatePlayerHealthUI(); // UI도 업데이트
        Debug.Log("플레이어 체력 모두 회복!");
    }

    public void RestoreSpecificHealth(float amount)
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySfx(SoundManager.Instance.healthRestoreSfx);
        }

        if (currentPlayerHp < maxPlayerHp) // 현재 체력이 최대 체력 미만일 때만 회복
        {


            currentPlayerHp += amount;
            currentPlayerHp = Mathf.Clamp(currentPlayerHp, 0, maxPlayerHp); // 최대 체력을 넘지 않도록
            UpdatePlayerHealthUI(); // 체력 UI 업데이트
            Debug.Log($"플레이어 체력 {amount} 회복! 현재 체력: {currentPlayerHp}");
        }
        else
        {
            Debug.Log("플레이어 체력이 이미 최대입니다.");
        }
    }


    public void ActivateShield()
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySfx(SoundManager.Instance.shieldActivateSfx);
        }

        // 이미 방어막이 활성화 상태라면 아무것도 하지 않음 (최대 1개)
        if (isShieldActive)
        {
            Debug.Log("이미 방어막이 활성화되어 있습니다.");
            return;
        }




        isShieldActive = true;
        if (shieldVisualEffect != null)
        {
            shieldVisualEffect.SetActive(true);
        }
        Debug.Log("방어막 활성화!");

        if (shieldActiveIcon != null)
        {
            shieldActiveIcon.SetActive(true);
        }


    }

    private void DeactivateShield()
    {
        isShieldActive = false;
        if (shieldVisualEffect != null)
        {
            shieldVisualEffect.SetActive(false);
        }

        // 여기에 '방어막 파괴' 효과음을 넣습니다.
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySfx(SoundManager.Instance.shieldBreakSfx); // SoundManager에 이 클립 추가 필요
        }

        if (shieldActiveIcon != null)
        {
            shieldActiveIcon.SetActive(false);
        }
    }

    public void ResetPlayerStateForNewGame()
    {
        Debug.Log("PlayerControl: 플레이어 상태를 새 게임에 맞게 리셋합니다.");

        // 1. 능력치(스탯) 초기화
        currentPlayerHp = maxPlayerHp;
        currentStamina = maxStamina;

        // 2. 아이템 및 상태 초기화
        hasStone = false;
        isShieldActive = false;

        // 3. 시각 효과(Visuals) 초기화
        if (heldStoneVisual != null)
        {
            heldStoneVisual.SetActive(false); // 손에 든 돌멩이 모델 비활성화
        }
        if (shieldVisualEffect != null)
        {
            shieldVisualEffect.SetActive(false); // 방어막 이펙트 비활성화
        }

        // 4. UI 초기화
        // 체력 및 스태미나 바를 가득 찬 상태로 업데이트
        UpdatePlayerHealthUI();
        UpdateStaminaUI();

        // 상태 아이콘 비활성화
        if (stoneLoadedIcon != null)
        {
            stoneLoadedIcon.SetActive(false);
        }
        if (shieldActiveIcon != null)
        {
            shieldActiveIcon.SetActive(false);
        }

        // 5. 플레이어 위치 및 상태 초기화 

        this.step = STEP.RUN;
        this.next_step = STEP.NONE;
        this.GetComponent<Rigidbody>().velocity = Vector3.zero; // 물리 속도 초기화
    }

    public float GetCurrentSpeed()
    {
        return current_speed;
    }

}